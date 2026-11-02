using System;
using Honeywell.Printer;
using PM45DemoV05.Models;
using PM45DemoV05.Printing;

namespace PM45DemoV05
{
    internal static class Program
    {
        private static UI.Canvas canvas;
        private static UI.Canvas.Text statusText;
        private static UI.Canvas.Text selectedDayText;
        private static UI.Canvas.Text selectionText;
        private static PrinterService printer;
        private static WeekdaySelection selection = new WeekdaySelection();
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
                if (!canvas.HasDisplay)
                {
                    canvas.Dispose();
                    return 1;
                }

                printer = new PrinterService();
                BuildMainScreen("Seleccione un día");
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
            canvas += new UI.Canvas.Text(20, 10, "PM45 - DEMO V5", "Univers", 20, White);
            statusText = new UI.Canvas.Text(20, 35, status, "Univers", 13, White);
            canvas += statusText;

            canvas += new UI.Canvas.Text(12, 78, "DÍA DE LA SEMANA", "Univers", 12, White);
            UI.Canvas.Rectangle field = new UI.Canvas.Rectangle(140, 68, 82, 35, FieldBackground);
            field.MouseDown += BeginDayEdit;
            canvas += field;

            if (selection.SelectedDay.Length > 0)
            {
                selectedDayText = new UI.Canvas.Text(148, 75, selection.SelectedDay, "Univers", 11, FieldText);
                canvas += selectedDayText;
            }

            AddButton(15, 250, 100, 45, "IMPRIMIR", Blue, PrintButtonPressed);
            AddButton(125, 250, 100, 45, "SALIR", Gray, ExitButtonPressed);
            canvas.Refresh();
        }

        private static void ShowWeekdayKeyboard()
        {
            canvas.Clear();
            canvas += new UI.Canvas.Rectangle(0, 0, canvas.Width, canvas.Height, Background);
            canvas += new UI.Canvas.Text(15, 12, "DÍA DE LA SEMANA", "Univers", 15, White);
            canvas += new UI.Canvas.Rectangle(145, 8, 78, 35, FieldBackground);

            for (int index = 0; index < selection.Days.Count; index++)
            {
                string day = selection.Days[index];
                AddButton(15, 52 + index * 39, 210, 32, day, day == selection.SelectedDay ? Blue : Gray, delegate { WeekdayPressed(day); });
            }

            AddButton(15, 255, 100, 40, "BORRAR", Gray, ClearEdit);
            AddButton(125, 255, 100, 40, "ENTER", Blue, ConfirmEdit);

            selectionText = new UI.Canvas.Text(153, 15, selection.SelectedDay, "Univers", 11, FieldText);
            canvas += selectionText;
            canvas.Refresh();
        }

        private static void BeginDayEdit(object sender, UI.Canvas.MouseEventArgs args)
        {
            ShowWeekdayKeyboard();
        }

        private static void WeekdayPressed(string day)
        {
            selection.Select(day);
            selectionText.Data = selection.SelectedDay;
            canvas.Refresh();
        }

        private static void ClearEdit(object sender, UI.Canvas.MouseEventArgs args)
        {
            selection.Clear();
            selectionText.Data = "";
            canvas.Refresh();
        }

        private static void ConfirmEdit(object sender, UI.Canvas.MouseEventArgs args)
        {
            if (!selection.Confirm())
            {
                selectionText.Data = "SELECCIONE";
                canvas.Refresh();
                return;
            }

            BuildMainScreen("Día aceptado");
        }

        private static void PrintButtonPressed(object sender, UI.Canvas.MouseEventArgs args)
        {
            if (printing)
            {
                return;
            }

            if (!selection.Confirm())
            {
                statusText.Data = "Seleccione un día";
                canvas.Refresh();
                return;
            }

            printing = true;
            statusText.Data = "Imprimiendo...";
            canvas.Refresh();

            try
            {
                State result = printer.Print(selection.SelectedDay);
                statusText.Data = result == State.NoError ? "Impresión terminada" : "Error: " + result;
            }
            catch (GeneralException ex)
            {
                statusText.Data = "Error de impresión";
                Console.WriteLine("GeneralException: " + ex.Message);
                Console.WriteLine("ErrorCode: " + ex.ErrorCode);
            }
            catch (Exception ex)
            {
                statusText.Data = "Error de impresión";
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
            UI.Canvas.Text label = new UI.Canvas.Text(x + 8, y + 8, text, "Univers", text.Length > 8 ? 10 : 14, White);
            label.MouseDown += handler;
            canvas += label;
        }
    }
}
