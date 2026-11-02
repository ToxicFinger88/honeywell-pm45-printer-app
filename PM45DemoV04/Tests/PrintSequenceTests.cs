using System;
using PM45DemoV04.Models;

namespace PM45DemoV04.Tests
{
    internal static class PrintSequenceTests
    {
        private static void AssertEqual(int expected, int actual, string message)
        {
            if (expected != actual)
            {
                throw new Exception(message + ": esperado " + expected + ", obtenido " + actual);
            }
        }

        private static void Main()
        {
            PrintSequence sequence = new PrintSequence(1, 10);

            AssertEqual(1, sequence.StartNumber, "Inicio");
            AssertEqual(10, sequence.EndNumber, "Final");
            AssertEqual(1, sequence.CurrentNumber, "Primer correlativo");

            sequence.Advance();
            AssertEqual(2, sequence.CurrentNumber, "Segundo correlativo");

            PrintSequence rollover = new PrintSequence(999, 2);
            AssertEqual(999, rollover.CurrentNumber, "Rollover inicial");
            AssertEqual(1, rollover.EndNumber, "Final con rollover");
            rollover.Advance();
            AssertEqual(1, rollover.CurrentNumber, "Rollover a uno");

            NumericInputBuffer input = new NumericInputBuffer();
            input.Append("1");
            input.Append("2");
            input.Append("3");
            input.Append("4");
            AssertEqual(123, Int32.Parse(input.Value), "Maximo de tres digitos");
            input.Backspace();
            AssertEqual(12, Int32.Parse(input.Value), "Borrar ultimo digito");

            PrintSequence labelSequence = new PrintSequence(1, 10);
            AssertEqualText("DEMO VERSION 4", LabelText.Title, "Titulo de etiqueta");
            AssertEqualText("NUMERO DE INICIO: 1", LabelText.Start(labelSequence), "Inicio de etiqueta");
            AssertEqualText("NUMERO FINAL: 10", LabelText.End(labelSequence), "Final de etiqueta");
            AssertEqualText("CORRELATIVO: 1", LabelText.Current(labelSequence), "Correlativo de etiqueta");
        }

        private static void AssertEqualText(string expected, string actual, string message)
        {
            if (expected != actual)
            {
                throw new Exception(message + ": esperado '" + expected + "', obtenido '" + actual + "'");
            }
        }
    }
}
