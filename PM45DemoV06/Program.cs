using System;
using Honeywell.Printer;
using PM45DemoV06.Models;
using PM45DemoV06.Printing;

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
                canvas.Run();
                return 0;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); return 1; }
            finally
            {
                if (printer != null) printer.Dispose();
                if (canvas != null) canvas.Dispose();
            }
        }

        private static void BuildMainScreen(string status)
        {
            canvas.Clear();
            canvas += new UI.Canvas.Rectangle(0, 0, canvas.Width, canvas.Height, Background);
            canvas += new UI.Canvas.Text(12, 7, "PM45 - DEMO V6", "Univers", 19, White);
            statusText = new UI.Canvas.Text(12, 30, status, "Univers", 11, White);
            canvas += statusText;
            AddFieldLabel(10, 58, "DÍA DE LA SEMANA", 12, 45, 115, daySelection.SelectedDay, 10, BeginDayEdit);
            AddFieldLabel(10, 103, "NUMERO INICIO", 12, 90, 115, startValue, 16, BeginStartEdit);
            AddFieldLabel(10, 148, "CANTIDAD", 12, 135, 115, quantityValue, 16, BeginQuantityEdit);
            AddButton(15, 250, 100, 45, "IMPRIMIR", Blue, PrintButtonPressed);
            AddButton(125, 250, 100, 45, "SALIR", Gray, ExitButtonPressed);
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

        private static void BeginDayEdit(object sender, UI.Canvas.MouseEventArgs args) { ShowDayScreen(); }

        private static void ShowDayScreen()
        {
            canvas.Clear();
            canvas += new UI.Canvas.Rectangle(0, 0, canvas.Width, canvas.Height, Background);
            canvas += new UI.Canvas.Text(15, 10, "DÍA DE LA SEMANA", "Univers", 15, White);
            canvas += new UI.Canvas.Rectangle(145, 8, 78, 35, FieldBackground);
            for (int i = 0; i < daySelection.Days.Count; i++)
            {
                string day = daySelection.Days[i];
                AddButton(15, 52 + i * 39, 210, 32, day, day == daySelection.SelectedDay ? Blue : Gray, delegate { SelectDay(day); });
            }
            AddButton(15, 255, 100, 40, "BORRAR", Gray, ClearDay);
            AddButton(125, 255, 100, 40, "ENTER", Blue, ConfirmDay);
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

        private static void AddButton(int x, int y, int width, int height, string text, Color color, UI.Canvas.MouseEventHandler handler)
        {
            UI.Canvas.Rectangle rectangle = new UI.Canvas.Rectangle(x, y, width, height, color);
            rectangle.MouseDown += handler; canvas += rectangle;
            UI.Canvas.Text label = new UI.Canvas.Text(x + 8, y + 8, text, "Univers", text.Length > 8 ? 10 : 14, White);
            label.MouseDown += handler; canvas += label;
        }
    }
}
