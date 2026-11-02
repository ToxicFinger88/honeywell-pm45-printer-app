using System;
using PM45DemoV05.Models;

namespace PM45DemoV05.Tests
{
    internal static class WeekdaySelectionTests
    {
        private static void AssertEqual(string expected, string actual, string message)
        {
            if (expected != actual)
            {
                throw new Exception(message + ": esperado '" + expected + "', obtenido '" + actual + "'");
            }
        }

        private static void AssertTrue(bool value, string message)
        {
            if (!value)
            {
                throw new Exception(message);
            }
        }

        private static void Main()
        {
            WeekdaySelection selection = new WeekdaySelection();
            AssertEqual("LUNES", selection.Days[0], "Primer día");
            AssertEqual("MARTES", selection.Days[1], "Segundo día");
            AssertEqual("MIÉRCOLES", selection.Days[2], "Tercer día");
            AssertEqual("JUEVES", selection.Days[3], "Cuarto día");
            AssertEqual("VIERNES", selection.Days[4], "Quinto día");
            AssertEqual("", selection.SelectedDay, "Selección inicial vacía");
            AssertEqual("DEMO VERSION 5", LabelText.Title, "Título de etiqueta");

            selection.Select("JUEVES");
            AssertEqual("JUEVES", selection.SelectedDay, "Selección visible");
            AssertTrue(selection.Confirm(), "La selección debe poder confirmarse");
            AssertEqual("DIA: JUEVES", LabelText.Day(selection.SelectedDay), "Día de etiqueta");

            selection.Clear();
            AssertEqual("", selection.SelectedDay, "BORRAR debe limpiar");
            AssertTrue(!selection.Confirm(), "No se debe confirmar una selección vacía");
        }
    }
}
