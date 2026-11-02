using System;
using Honeywell.Printer;
using PM45DemoV041.Models;
using PM45DemoV041.Printing;

namespace PM45DemoV041
{
    internal static class Program
    {
        private static UI.Canvas canvas;
        private static UI.Canvas.Text statusText;
        private static UI.Canvas.Text startText;
        private static UI.Canvas.Text quantityText;
        private static PrinterService printer;
        private static string startValue = "";
        private static string quantityValue = "";
        private static string editingValue = "";
        private static int editingField;
        private static bool printing;
        private static UI.Canvas.Text keyboardValueText;
        private static NumericInputBuffer inputBuffer = new NumericInputBuffer();

        private static readonly Color Background = new Color(20, 32, 48, 255);
        private static readonly Color White = new Color(255, 255, 255, 255);
        private static readonly Color Blue = new Color(20, 120, 200, 255);
        private static readonly Color Gray = new Color(85, 100, 115, 255);
        private static readonly Color DarkBlue = new Color(35, 65, 90, 255);
        private static readonly Color FieldBackground = new Color(35, 65, 90, 255);
        private static readonly Color FieldText = new Color(255, 255, 0, 255);

        private static int Main(string[] args)
        {
            try
            {
                canvas = new UI.Canvas();
                if (!canvas.HasDisplay)
                {
                    canvas.Dispose();
                    return 1;
                }

                printer = new PrinterService();
                BuildMainScreen("Ingrese los valores");
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
                if (printer != null)
                {
                    printer.Dispose();
                }

                if (canvas != null)
                {
                    canvas.Dispose();
                }
            }
        }

        private static void BuildMainScreen(string status)
        {
            canvas.Clear();
            canvas += new UI.Canvas.Rectangle(0, 0, canvas.Width, canvas.Height, Background);
            canvas += new UI.Canvas.Text(20, 10, "PM45 - DEMO V4.1", "Univers", 20, White);
            statusText = new UI.Canvas.Text(20, 35, status, "Univers", 13, White);
            canvas += statusText;

            canvas += new UI.Canvas.Text(12, 78, "NUMERO INICIO", "Univers", 13, White);
            startText = new UI.Canvas.Text(148, 75, DisplayValue(startValue), "Univers", 18, FieldText);
            AddField(140, 68, 82, 35, startText, BeginStartEdit);

            canvas += new UI.Canvas.Text(12, 123, "CANTIDAD", "Univers", 13, White);
            quantityText = new UI.Canvas.Text(148, 120, DisplayValue(quantityValue), "Univers", 18, FieldText);
            AddField(140, 113, 82, 35, quantityText, BeginQuantityEdit);

            AddButton(15, 250, 100, 45, "IMPRIMIR", Blue, PrintButtonPressed);
            AddButton(125, 250, 100, 45, "SALIR", Gray, ExitButtonPressed);

            if (startValue.Length > 0)
            {
                canvas += new UI.Canvas.Text(155, 75, startValue, "Univers", 18, FieldText);
            }

            if (quantityValue.Length > 0)
            {
                canvas += new UI.Canvas.Text(155, 120, quantityValue, "Univers", 18, FieldText);
            }
            canvas.Refresh();
        }

        private static void ShowNumericKeyboard()
        {
            canvas.Clear();
            canvas += new UI.Canvas.Rectangle(0, 0, canvas.Width, canvas.Height, Background);
            canvas += new UI.Canvas.Text(15, 15, editingField == 1 ? "NUMERO INICIO" : "CANTIDAD", "Univers", 17, White);
            canvas += new UI.Canvas.Rectangle(145, 8, 78, 35, FieldBackground);

            string[] keys = { "7", "8", "9", "4", "5", "6", "1", "2", "3", "BORRAR", "0", "ENTER" };
            for (int index = 0; index < keys.Length; index++)
            {
                int row = index / 3;
                int column = index % 3;
                int x = 15 + column * 75;
                int y = 58 + row * 58;
                string key = keys[index];
                AddButton(x, y, 65, 48, key, key == "ENTER" ? Blue : Gray, delegate { NumericKeyPressed(key); });
            }

            keyboardValueText = new UI.Canvas.Text(155, 15, FieldDisplay.Format(editingValue), "Univers", 20, FieldText);
            canvas += keyboardValueText;
            RenderKeyboardValue();

            canvas.Refresh();
        }

        private static void BeginStartEdit(object sender, UI.Canvas.MouseEventArgs args)
        {
            editingField = 1;
            editingValue = startValue;
            inputBuffer.Set(editingValue);
            ShowNumericKeyboard();
        }

        private static void BeginQuantityEdit(object sender, UI.Canvas.MouseEventArgs args)
        {
            editingField = 2;
            editingValue = quantityValue;
            inputBuffer.Set(editingValue);
            ShowNumericKeyboard();
        }

        private static void NumericKeyPressed(string key)
        {
            if (key == "ENTER")
            {
                editingValue = inputBuffer.Value;
                ConfirmEdit();
                return;
            }

            if (key == "BORRAR")
            {
                inputBuffer.Backspace();
            }
            else
            {
                inputBuffer.Append(key);
            }

            editingValue = inputBuffer.Value;
            RenderKeyboardValue();
        }

        private static void RenderKeyboardValue()
        {
            keyboardValueText.Data = FieldDisplay.Format(editingValue);
            canvas.Refresh();
        }

        private static void ConfirmEdit()
        {
            int value;
            if (!Int32.TryParse(editingValue, out value) || value < 1 || value > 999)
            {
                ShowNumericKeyboard();
                return;
            }

            if (editingField == 1)
            {
                startValue = value.ToString();
            }
            else
            {
                quantityValue = value.ToString();
            }

            BuildMainScreen("Valores aceptados");
        }

        private static void PrintButtonPressed(object sender, UI.Canvas.MouseEventArgs args)
        {
            if (printing)
            {
                return;
            }

            int start;
            int quantity;
            if (!Int32.TryParse(startValue, out start) || !Int32.TryParse(quantityValue, out quantity))
            {
                statusText.Data = "Complete los dos valores";
                canvas.Refresh();
                return;
            }

            printing = true;
            statusText.Data = "Imprimiendo...";
            canvas.Refresh();

            try
            {
                PrintSequence sequence = new PrintSequence(start, quantity);
                State result = printer.Print(sequence);
                statusText.Data = result == State.NoError ? "Impresion terminada" : "Error: " + result;
            }
            catch (GeneralException ex)
            {
                statusText.Data = "Error de impresion";
                Console.WriteLine("GeneralException: " + ex.Message);
                Console.WriteLine("ErrorCode: " + ex.ErrorCode);
            }
            catch (Exception ex)
            {
                statusText.Data = "Error de impresion";
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                printing = false;
                canvas.Refresh();
            }
        }

        private static void ExitButtonPressed(object sender, UI.Canvas.MouseEventArgs args)
        {
            canvas.Exit();
        }

        private static void AddButton(int x, int y, int width, int height, string text, Color color, UI.Canvas.MouseEventHandler handler)
        {
            UI.Canvas.Rectangle rectangle = new UI.Canvas.Rectangle(x, y, width, height, color);
            rectangle.MouseDown += handler;
            canvas += rectangle;
            UI.Canvas.Text label = new UI.Canvas.Text(x + 8, y + 13, text, "Univers", text.Length > 6 ? 12 : 16, White);
            label.MouseDown += handler;
            canvas += label;
        }

        private static void AddField(int x, int y, int width, int height, UI.Canvas.Text text, UI.Canvas.MouseEventHandler handler)
        {
            UI.Canvas.Rectangle rectangle = new UI.Canvas.Rectangle(x, y, width, height, FieldBackground);
            rectangle.MouseDown += handler;
            canvas += rectangle;
        }

        private static string DisplayValue(string value)
        {
            return FieldDisplay.Format(value);
        }
    }
}
