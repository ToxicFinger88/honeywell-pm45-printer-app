using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Honeywell.Printer;
using PM45KeyboardTest1.Models;

namespace PM45KeyboardTest1
{
    internal static class Program
    {
        private const int HistoryLimit = 5;

        private static UI.Canvas canvas;
        private static UI.Canvas.Timer timer;
        private static PrintControl printer;
        private static string printStatus;
        private static Communication.USBHost usbHost;
        private static FileStream usbStream;
        private static readonly KeyboardInputModel inputModel = new KeyboardInputModel();
        private static readonly Queue<byte[]> packets = new Queue<byte[]>();
        private static readonly Queue<string> statusQueue = new Queue<string>();
        private static readonly List<string> history = new List<string>();
        private static readonly object queueLock = new object();
        private static volatile bool reading;

        private static UI.Canvas.Text statusText;
        private static UI.Canvas.Text connectionText;
        private static UI.Canvas.Text textValueText;
        private static UI.Canvas.Text numberValueText;
        private static UI.Canvas.Text rawText;
        private static UI.Canvas.Text historyText;
        private static UI.Canvas.Rectangle textFieldRectangle;
        private static UI.Canvas.Rectangle numberFieldRectangle;

        private static string connectionStatus = "USB: buscando...";
        private static string lastRaw = "RAW: -";

        private static readonly Color Background = new Color(20, 32, 48, 255);
        private static readonly Color White = new Color(255, 255, 255, 255);
        private static readonly Color Blue = new Color(20, 120, 200, 255);
        private static readonly Color Gray = new Color(85, 100, 115, 255);
        private static readonly Color FieldBackground = new Color(35, 65, 90, 255);
        private static readonly Color FieldText = new Color(255, 255, 0, 255);

        private static int Main(string[] args)
        {
            try
            {
                canvas = new UI.Canvas();
                if (!canvas.HasDisplay) return 1;
                printer = new PrintControl();

                BuildScreen("Campo seleccionado: TEXTO");
                OpenUsbHost();

                timer = new UI.Canvas.Timer();
                timer.Interval = 100;
                timer.Tick += TimerTick;
                timer.Start();

                canvas.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 1;
            }
            finally
            {
                reading = false;
                CloseUsb();
                if (timer != null) timer.Dispose();
                if (printer != null) printer.Dispose();
                if (canvas != null) canvas.Dispose();
            }
        }

        private static void BuildScreen(string status)
        {
            canvas.Clear();
            canvas += new UI.Canvas.Rectangle(0, 0, canvas.Width, canvas.Height, Background);
            canvas += new UI.Canvas.Text(8, 5, "TECLADO USB 2", "Univers", 14, White);

            connectionText = new UI.Canvas.Text(8, 25, connectionStatus, "Univers", 8, White);
            canvas += connectionText;
            statusText = new UI.Canvas.Text(8, 39, status, "Univers", 8, White);
            canvas += statusText;

            canvas += new UI.Canvas.Text(8, 57, "TEXTO", "Univers", 10, White);
            textFieldRectangle = AddField(92, 53, 135, 29, TextFieldPressed, out textValueText);
            canvas += new UI.Canvas.Text(8, 95, "NUMERO", "Univers", 10, White);
            numberFieldRectangle = AddField(92, 91, 135, 29, NumberFieldPressed, out numberValueText);

            canvas += new UI.Canvas.Text(8, 132, "ULTIMO BYTE", "Univers", 8, White);
            rawText = new UI.Canvas.Text(8, 146, lastRaw, "Univers", 8, FieldText);
            canvas += rawText;

            canvas += new UI.Canvas.Text(8, 165, "HISTORIAL", "Univers", 8, White);
            historyText = new UI.Canvas.Text(8, 178, "", "Univers", 7, White);
            canvas += historyText;

            AddButton(15, 220, 210, 35, "IMPRIMIR", Blue, PrintPressed);
            AddButton(15, 260, 100, 40, "LIMPIAR", Gray, ClearPressed);
            AddButton(125, 260, 100, 40, "SALIR", Blue, ExitPressed);
            RefreshScreen();
        }

        private static UI.Canvas.Rectangle AddField(int x, int y, int width, int height, UI.Canvas.MouseEventHandler handler, out UI.Canvas.Text valueText)
        {
            UI.Canvas.Rectangle rectangle = new UI.Canvas.Rectangle(x, y, width, height, FieldBackground);
            rectangle.MouseDown += handler;
            canvas += rectangle;

            valueText = new UI.Canvas.Text(x + 7, y + 7, "", "Univers", 12, FieldText);
            valueText.MouseDown += handler;
            canvas += valueText;
            return rectangle;
        }

        private static void OpenUsbHost()
        {
            try
            {
                string[] ports = Communication.USBHost.GetPortNames();
                if (ports == null || ports.Length == 0)
                {
                    QueueStatus("USB: sin teclado");
                    return;
                }

                for (int index = 0; index < ports.Length; index++)
                {
                    try
                    {
                        usbHost = new Communication.USBHost(ports[index]);
                        usbHost.Open();
                        if (!usbHost.IsOpen)
                        {
                            CloseUsb();
                            continue;
                        }

                        usbStream = usbHost.GetStream();
                        reading = true;
                        QueueStatus("USB: conectado " + ports[index] + " " + usbHost.HIDName);
                        ThreadPool.QueueUserWorkItem(ReadUsbHost);
                        return;
                    }
                    catch (Exception ex)
                    {
                        QueueStatus("USB: error " + ShortMessage(ex.Message));
                        CloseUsb();
                    }
                }

                QueueStatus("USB: ningún puerto pudo abrirse");
            }
            catch (Exception ex)
            {
                QueueStatus("USB: error " + ShortMessage(ex.Message));
                CloseUsb();
            }
        }

        private static void ReadUsbHost(object state)
        {
            FileStream stream = usbStream;
            try
            {
                UsbReadLoop.Run(
                    delegate(byte[] buffer) { return stream.Read(buffer, 0, buffer.Length); },
                    delegate { return reading; },
                    delegate { Thread.Sleep(100); },
                    delegate(byte[] packet) { lock (queueLock) packets.Enqueue(packet); });
            }
            catch (Exception ex)
            {
                if (reading) QueueStatus("USB: error de lectura " + ShortMessage(ex.Message));
                reading = false;
            }
        }

        private static void TimerTick(object sender, UI.Canvas.TimerEventArgs args)
        {
            while (true)
            {
                string message;
                lock (queueLock)
                {
                    if (statusQueue.Count == 0) break;
                    message = statusQueue.Dequeue();
                }
                connectionStatus = message;
            }

            while (true)
            {
                byte[] packet;
                lock (queueLock)
                {
                    if (packets.Count == 0) break;
                    packet = packets.Dequeue();
                }
                ProcessPacket(packet);
            }

            RefreshScreen();
        }

        private static void ProcessPacket(byte[] packet)
        {
            for (int index = 0; index < packet.Length; index++)
            {
                byte value = packet[index];
                lastRaw = KeyboardByteFormatter.Describe(value);
                history.Insert(0, lastRaw);
                while (history.Count > HistoryLimit) history.RemoveAt(history.Count - 1);
                inputModel.ProcessByte(value);
                printStatus = null;
            }
        }

        private static void TextFieldPressed(object sender, UI.Canvas.MouseEventArgs args)
        {
            inputModel.SelectField(InputField.Text);
            RefreshScreen();
        }

        private static void NumberFieldPressed(object sender, UI.Canvas.MouseEventArgs args)
        {
            inputModel.SelectField(InputField.Number);
            RefreshScreen();
        }

        private static void ClearPressed(object sender, UI.Canvas.MouseEventArgs args)
        {
            inputModel.Clear();
            printStatus = null;
            history.Clear();
            lastRaw = "RAW: -";
            RefreshScreen();
        }

        private static void ExitPressed(object sender, UI.Canvas.MouseEventArgs args)
        {
            reading = false;
            CloseUsb();
            canvas.Exit();
        }

        private static void RefreshScreen()
        {
            if (connectionText == null) return;
            connectionText.Data = connectionStatus;
            statusText.Data = printStatus ?? inputModel.LastStatus;
            textValueText.Data = inputModel.TextValue;
            numberValueText.Data = inputModel.NumberValue;
            rawText.Data = lastRaw;
            historyText.Data = string.Join("\n", history.ToArray());
            textFieldRectangle.Color = inputModel.ActiveField == InputField.Text ? Blue : FieldBackground;
            numberFieldRectangle.Color = inputModel.ActiveField == InputField.Number ? Blue : FieldBackground;
            canvas.Refresh();
        }

        private static void QueueStatus(string message)
        {
            lock (queueLock) statusQueue.Enqueue(message);
        }

        private static void PrintPressed(object sender, UI.Canvas.MouseEventArgs args)
        {
            printStatus = "Enviando impresion...";
            RefreshScreen();
            try
            {
                using (Drawing label = new Drawing())
                {
                    label.Height = (int)Math.Round(printer.PrintheadResolution * 150.0 / 25.4);
                    AddLabelText(label, 730, "PRUEBA TECLADO USB 2", 22);
                    AddLabelText(label, 600, "TEXTO: " + inputModel.TextValue, 18);
                    AddLabelText(label, 500, "NUMERO: " + inputModel.NumberValue, 18);
                    label.PartialRendering = true;
                    State result = printer.PrintFeed(label, 1);
                    printStatus = result == State.NoError ? "Impresion enviada" : "Error: " + result;
                }
            }
            catch (Exception ex)
            {
                printStatus = "Error de impresion";
                Console.WriteLine(ex.ToString());
            }
            RefreshScreen();
        }

        private static void AddLabelText(Drawing label, int y, string value, int height)
        {
            Drawing.Text text = new Drawing.Text();
            text.Point = new Point(100, y);
            text.Data = value;
            text.Height = height;
            label.AddObject(text);
        }

        private static string ShortMessage(string message)
        {
            if (String.IsNullOrEmpty(message)) return "sin detalle";
            return message.Length <= 35 ? message : message.Substring(0, 35);
        }

        private static void CloseUsb()
        {
            if (usbStream != null)
            {
                try { usbStream.Close(); } catch { }
                try { usbStream.Dispose(); } catch { }
                usbStream = null;
            }

            if (usbHost != null)
            {
                try { usbHost.Close(); } catch { }
                try { usbHost.Dispose(); } catch { }
                usbHost = null;
            }
        }

        private static void AddButton(int x, int y, int width, int height, string text, Color color, UI.Canvas.MouseEventHandler handler)
        {
            UI.Canvas.Rectangle rectangle = new UI.Canvas.Rectangle(x, y, width, height, color);
            rectangle.MouseDown += handler;
            canvas += rectangle;

            UI.Canvas.Text label = new UI.Canvas.Text(x + 8, y + 10, text, "Univers", 11, White);
            label.MouseDown += handler;
            canvas += label;
        }
    }
}
