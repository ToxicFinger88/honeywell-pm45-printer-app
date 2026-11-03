using System;
using PM45DemoV06.Models;

namespace PM45DemoV06.Tests
{
    internal static class PrintSequenceV06Tests
    {
        private static void AssertEqual(string expected, string actual, string message)
        {
            if (expected != actual) throw new Exception(message + ": esperado '" + expected + "', obtenido '" + actual + "'");
        }

        private static void Main()
        {
            WeekdaySelection days = new WeekdaySelection();
            AssertEqual("LUNES", days.Days[0], "Orden de días");
            days.Select("VIERNES");
            AssertEqual("VIERNES", days.SelectedDay, "Día seleccionado");
            AssertEqual("DEMO VERSION 6", LabelText.Title, "Título V6");
            AssertEqual("DIA: VIERNES", LabelText.Day(days.SelectedDay), "Día de etiqueta");
            PrintSequence sequence = new PrintSequence(998, 3);
            AssertEqual("NUMERO FINAL: 1", LabelText.End(sequence), "Rollover de etiqueta");
            AssertEqual("CORRELATIVO: 998", LabelText.Current(sequence), "Correlativo inicial");
            sequence.Advance(); sequence.Advance();
            AssertEqual("1", sequence.CurrentNumber.ToString(), "Rollover 999 a 1");
        }
    }
}
