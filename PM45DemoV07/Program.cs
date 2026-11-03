using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Honeywell.Printer;
using PM45DemoV06.Models;
using PM45DemoV06.Printing;
using PM45KeyboardTest1.Models;

namespace PM45DemoV06
{
    internal static class Program
    {
        private static UI.Canvas canvas;
        private static UI.Canvas.Text statusText;
        private static UI.Canvas.Text keyboardValueText;
        private static PrinterService printer;
        private static WeekdaySelection daySelection = new WeekdaySelection();
        private static NumericInputBuffer inputBuffer = new NumericInputBuffer();
        private static string startValue = "";
        private static string quantityValue = "";
        private static int editingField;
        private static bool printing;
        private static Communication.USBHost usb;
        private static FileStream stream;
        private static UI.Canvas.Timer timer;
        private static volatile bool reading;
        private static readonly Queue<byte> keys = new Queue<byte>();
        private static readonly object sync = new object();
        private static int screen;
        private static int focus;
        private static string usbStatus = "USB: buscando";

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
                printer = new PrinterService();
                BuildMainScreen("Ingrese los valores");
                OpenUsb();
                timer = new UI.Canvas.Timer(); timer.Interval = 100; timer.Tick += Tick; timer.Start();
                canvas.Run();
                return 0;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); return 1; }
            finally
            {
                reading = false;
                if (stream != null) stream.Dispose();
                if (usb != null) usb.Dispose();
                if (timer != null) timer.Dispose();
                if (printer != null) printer.Dispose();
                if (canvas != null) canvas.Dispose();
            }
        }

        private static void BuildMainScreen(string status)
        {
            screen = 0;
            if (focus < 0 || focus > 4) focus = 0;
            canvas.Clear();
            canvas += new UI.Canvas.Rectangle(0, 0, canvas.Width, canvas.Height, Background);
            canvas += new UI.Canvas.Text(12, 7, "PM45 - DEMO V7", "Univers", 19, White);
            statusText = new UI.Canvas.Text(12, 30, status, "Univers", 11, White);
            canvas += statusText;
            AddFieldLabel(10, 58, "DÍA DE LA SEMANA", 12, 45, 115, daySelection.SelectedDay, 10, BeginDayEdit);
            AddFieldLabel(10, 103, "NUMERO INICIO", 12, 90, 115, startValue, 16, BeginStartEdit);
            AddFieldLabel(10, 148, "CANTIDAD", 12, 135, 115, quantityValue, 16, BeginQuantityEdit);
            AddButton(15, 250, 100, 45, "IMPRIMIR", Blue, PrintButtonPressed);
            AddButton(125, 250, 100, 45, "SALIR", Gray, ExitButtonPressed);
            canvas += new UI.Canvas.Text(10, 205, "TAB: " + new string[] { "DIA", "INICIO", "CANTIDAD", "IMPRIMIR", "SALIR" }[focus], "Univers", 11, FieldText);
            canvas += new UI.Canvas.Text(10, 225, usbStatus, "Univers", 8, White);
            canvas.Refresh();
        }

        private static void AddFieldLabel(int labelX, int labelY, string label, int fontSize, int fieldY, int fieldX, string value, int valueSize, UI.Canvas.MouseEventHandler handler)
        {
            canvas += new UI.Canvas.Text(labelX, labelY, label, "Univers", fontSize, White);
            UI.Canvas.Rectangle field = new UI.Canvas.Rectangle(fieldX, fieldY, 105, 35, FieldBackground);
            field.MouseDown += handler;
            canvas += field;
            if (value.Length > 0) canvas += new UI.Canvas.Text(fieldX + 8, fieldY + 8, value, "Univers", valueSize, FieldText);
        }

        private static void BeginDayEdit(object sender, UI.Canvas.MouseEventArgs args) { focus = 0; ShowDayScreen(); }

        private static void ShowDayScreen()
        {
            screen = 1;
            canvas.Clear();
            canvas += new UI.Canvas.Rectangle(0, 0, canvas.Width, canvas.Height, Background);
            canvas += new UI.Canvas.Text(15, 10, "DÍA DE LA SEMANA", "Univers", 15, White);
            canvas += new UI.Canvas.Rectangle(145, 8, 78, 35, FieldBackground);
            for (int i = 0; i < daySelection.Days.Count; i++)
            {
                string day = daySelection.Days[i];
                AddButton(15, 52 + i * 39, 210, 32, day, i == focus ? Blue : Gray, delegate { SelectDay(day); });
            }
            AddButton(15, 255, 100, 40, "BORRAR", focus == 5 ? Blue : Gray, ClearDay);
            AddButton(125, 255, 100, 40, "ENTER", focus == 6 ? Blue : Gray, ConfirmDay);
            keyboardValueText = new UI.Canvas.Text(153, 15, daySelection.SelectedDay, "Univers", 11, FieldText);
            canvas += keyboardValueText;
            canvas.Refresh();
        }

        private static void SelectDay(string day) { daySelection.Select(day); keyboardValueText.Data = daySelection.SelectedDay; canvas.Refresh(); }
        private static void ClearDay(object sender, UI.Canvas.MouseEventArgs args) { daySelection.Clear(); keyboardValueText.Data = ""; canvas.Refresh(); }
        private static void ConfirmDay(object sender, UI.Canvas.MouseEventArgs args)
        {
            if (!daySelection.Confirm()) { keyboardValueText.Data = "SELECCIONE"; canvas.Refresh(); return; }
            BuildMainScreen("Día aceptado");
        }

        private static void BeginStartEdit(object sender, UI.Canvas.MouseEventArgs args) { editingField = 2; ShowNumericScreen(startValue, "NUMERO INICIO"); }
        private static void BeginQuantityEdit(object sender, UI.Canvas.MouseEventArgs args) { editingField = 3; ShowNumericScreen(quantityValue, "CANTIDAD"); }

        private static void ShowNumericScreen(string value, string title)
        {
            screen = 2;
            inputBuffer.Set(value);
            canvas.Clear();
            canvas += new UI.Canvas.Rectangle(0, 0, canvas.Width, canvas.Height, Background);
            canvas += new UI.Canvas.Text(15, 10, title, "Univers", 17, White);
            canvas += new UI.Canvas.Rectangle(145, 8, 78, 35, FieldBackground);
            string[] keys = { "7", "8", "9", "4", "5", "6", "1", "2", "3", "BORRAR", "0", "ENTER" };
            for (int i = 0; i < keys.Length; i++)
            {
                int row = i / 3, col = i % 3;
                string key = keys[i];
                AddButton(15 + col * 75, 58 + row * 58, 65, 48, key, key == "ENTER" ? Blue : Gray, delegate { NumericPressed(key); });
            }
            keyboardValueText = new UI.Canvas.Text(155, 15, inputBuffer.Value, "Univers", 20, FieldText);
            canvas += keyboardValueText;
            canvas.Refresh();
        }

        private static void NumericPressed(string key)
        {
            if (key == "BORRAR") inputBuffer.Backspace();
            else if (key != "ENTER") inputBuffer.Append(key);
            if (key == "ENTER") { ConfirmNumber(); return; }
            keyboardValueText.Data = inputBuffer.Value;
            canvas.Refresh();
        }

        private static void ConfirmNumber()
        {
            int value;
            if (!Int32.TryParse(inputBuffer.Value, out value) || value < 1 || value > 999)
            {
                ShowNumericScreen(inputBuffer.Value, editingField == 2 ? "NUMERO INICIO" : "CANTIDAD");
                return;
            }
            if (editingField == 2) startValue = value.ToString(); else quantityValue = value.ToString();
            BuildMainScreen("Valores aceptados");
        }

        private static void PrintButtonPressed(object sender, UI.Canvas.MouseEventArgs args)
        {
            int start, quantity;
            if (!daySelection.Confirm() || !Int32.TryParse(startValue, out start) || !Int32.TryParse(quantityValue, out quantity))
            { statusText.Data = "Complete día, inicio y cantidad"; canvas.Refresh(); return; }
            if (printing) return;
            printing = true; statusText.Data = "Imprimiendo..."; canvas.Refresh();
            try
            {
                State result = printer.Print(new PrintSequence(start, quantity), daySelection.SelectedDay);
                statusText.Data = result == State.NoError ? "Impresión terminada" : "Error: " + result;
            }
            catch (GeneralException ex) { statusText.Data = "Error de impresión"; Console.WriteLine(ex.ToString()); }
            catch (Exception ex) { statusText.Data = "Error de impresión"; Console.WriteLine(ex.ToString()); }
            finally { printing = false; canvas.Refresh(); }
        }

        private static void ExitButtonPressed(object sender, UI.Canvas.MouseEventArgs args) { canvas.Exit(); }

        private static void OpenUsb()
        {
            try {
                string[] ports = Communication.USBHost.GetPortNames();
                foreach (string port in ports) {
                    try { usb = new Communication.USBHost(port); usb.Open(); if (!usb.IsOpen) { usb.Dispose(); usb = null; continue; }
                        stream = usb.GetStream(); reading = true; usbStatus = "USB abierto";
                        ThreadPool.QueueUserWorkItem(ReadUsb); return;
                    } catch (Exception ex) { Console.WriteLine(ex); if (usb != null) usb.Dispose(); usb = null; }
                }
                usbStatus = "USB no disponible";
            } catch (Exception ex) { usbStatus = "Error USB"; Console.WriteLine(ex); }
        }

        private static void ReadUsb(object unused)
        {
            FileStream source = stream;
            try { UsbReadLoop.Run(delegate(byte[] b) { return source.Read(b, 0, b.Length); }, delegate { return reading; }, delegate { Thread.Sleep(100); }, delegate(byte[] data) { lock (sync) foreach (byte b in data) keys.Enqueue(b); }); }
            catch (Exception ex) { if (reading) Console.WriteLine(ex); reading = false; }
        }

        private static void Tick(object sender, UI.Canvas.TimerEventArgs args)
        {
            while (true) { byte key; lock (sync) { if (keys.Count == 0) break; key = keys.Dequeue(); } HandleKey(key); }
        }

        private static void HandleKey(byte key)
        {
            if (screen == 0) {
                if (key == 9) { focus = (focus + 1) % 5; BuildMainScreen("Seleccione con ENTER"); }
                else if (key == 10 || key == 13) {
                    if (focus == 0) { focus = 0; ShowDayScreen(); }
                    else if (focus == 1) BeginStartEdit(null, null);
                    else if (focus == 2) BeginQuantityEdit(null, null);
                    else if (focus == 3) PrintButtonPressed(null, null);
                    else ExitButtonPressed(null, null);
                }
            } else if (screen == 1) {
                if (key == 9) { focus = (focus + 1) % 7; ShowDayScreen(); }
                else if (key == 10 || key == 13) {
                    if (focus < 5) { daySelection.Select(daySelection.Days[focus]); focus = 0; BuildMainScreen("Dia aceptado"); }
                    else if (focus == 5) { ClearDay(null, null); }
                    else { focus = 0; ConfirmDay(null, null); }
                }
            } else {
                if (key >= 48 && key <= 57) NumericPressed(((char)key).ToString());
                else if (key == 8 || key == 127) NumericPressed("BORRAR");
                else if (key == 10 || key == 13) { focus = editingField == 2 ? 1 : 2; NumericPressed("ENTER"); }
            }
        }

        private static void AddButton(int x, int y, int width, int height, string text, Color color, UI.Canvas.MouseEventHandler handler)
        {
            UI.Canvas.Rectangle rectangle = new UI.Canvas.Rectangle(x, y, width, height, color);
            rectangle.MouseDown += handler; canvas += rectangle;
            UI.Canvas.Text label = new UI.Canvas.Text(x + 8, y + 8, text, "Univers", text.Length > 8 ? 10 : 14, White);
            label.MouseDown += handler; canvas += label;
        }
    }
}
