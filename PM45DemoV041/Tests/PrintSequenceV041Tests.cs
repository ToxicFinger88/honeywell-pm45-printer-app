using System;
using PM45DemoV041.Models;

namespace PM45DemoV041.Tests
{
    internal static class PrintSequenceV041Tests
    {
        private static void AssertEqual(string expected, string actual, string message)
        {
            if (expected != actual)
            {
                throw new Exception(message + ": esperado '" + expected + "', obtenido '" + actual + "'");
            }
        }

        private static void Main()
        {
            AssertEqual("DEMO VERSION 4.1", LabelText.Title, "Version de la demo");
            AssertEqual("", FieldDisplay.Format(""), "Campo vacio sin guiones");
            AssertEqual("25", FieldDisplay.Format("25"), "Campo con valor visible");

            NumericInputBuffer input = new NumericInputBuffer();
            input.Append("1");
            input.Append("2");
            input.Backspace();
            AssertEqual("1", input.Value, "Borrar debe actualizar el valor inmediatamente");

            input.Backspace();
            AssertEqual("", input.Value, "Borrar debe limpiar el campo");

            input.Append("7");
            input.Append("5");
            AssertEqual("75", input.Value, "El nuevo valor debe sustituir al anterior");
        }
    }
}
