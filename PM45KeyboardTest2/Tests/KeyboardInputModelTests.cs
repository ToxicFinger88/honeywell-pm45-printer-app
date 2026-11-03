using System;
using PM45KeyboardTest1.Models;

namespace PM45KeyboardTest1.Tests
{
    internal static class KeyboardInputModelTests
    {
        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!Object.Equals(expected, actual))
            {
                throw new Exception(message + ": esperado '" + expected + "', obtenido '" + actual + "'");
            }
        }

        private static void AssertContains(string expectedPart, string actual, string message)
        {
            if (actual == null || actual.IndexOf(expectedPart, StringComparison.Ordinal) < 0)
            {
                throw new Exception(message + ": no se encontró '" + expectedPart + "' en '" + actual + "'");
            }
        }

        private static void AcceptsDigitsInNumberField()
        {
            KeyboardInputModel model = new KeyboardInputModel();
            model.SelectField(InputField.Number);
            model.ProcessByte((byte)'1');
            model.ProcessByte((byte)'2');
            AssertEqual("12", model.NumberValue, "Dígitos numéricos");
        }

        private static void RejectsNonDigitsInNumberField()
        {
            KeyboardInputModel model = new KeyboardInputModel();
            model.SelectField(InputField.Number);
            model.ProcessByte((byte)'a');
            model.ProcessByte((byte)'.');
            model.ProcessByte((byte)'-');
            model.ProcessByte((byte)' ');
            AssertEqual("", model.NumberValue, "Caracteres inválidos no deben agregarse");
            AssertContains("inválido", model.LastStatus, "Mensaje de dato inválido");
        }

        private static void BackspaceAndDeleteRemoveLastCharacter()
        {
            KeyboardInputModel model = new KeyboardInputModel();
            model.SelectField(InputField.Number);
            model.ProcessByte((byte)'1');
            model.ProcessByte((byte)'2');
            model.ProcessByte((byte)'3');
            model.ProcessByte(8);
            AssertEqual("12", model.NumberValue, "Backspace");
            model.ProcessByte(127);
            AssertEqual("1", model.NumberValue, "Delete");
        }

        private static void EnterConfirmsCurrentField()
        {
            KeyboardInputModel model = new KeyboardInputModel();
            model.SelectField(InputField.Number);
            model.ProcessByte((byte)'7');
            model.ProcessByte(13);
            AssertEqual("7", model.NumberValue, "Enter no debe modificar el valor");
            AssertContains("confirmado", model.LastStatus, "Mensaje de confirmación");
        }

        private static void TabSwitchesBetweenFields()
        {
            KeyboardInputModel model = new KeyboardInputModel();
            AssertEqual(InputField.Text, model.ActiveField, "Campo inicial");
            model.ProcessByte(9);
            AssertEqual(InputField.Number, model.ActiveField, "Primer Tab");
            model.ProcessByte(9);
            AssertEqual(InputField.Text, model.ActiveField, "Segundo Tab");
        }

        private static void TextFieldAcceptsPrintableCharacters()
        {
            KeyboardInputModel model = new KeyboardInputModel();
            model.ProcessByte((byte)'A');
            model.ProcessByte((byte)'1');
            model.ProcessByte((byte)'-');
            AssertEqual("A1-", model.TextValue, "Caracteres de texto");
            model.ProcessByte(8);
            AssertEqual("A1", model.TextValue, "Backspace de texto");
        }

        private static void ClearResetsBothFields()
        {
            KeyboardInputModel model = new KeyboardInputModel();
            model.ProcessByte((byte)'A');
            model.SelectField(InputField.Number);
            model.ProcessByte((byte)'2');
            model.Clear();
            AssertEqual("", model.TextValue, "Limpiar texto");
            AssertEqual("", model.NumberValue, "Limpiar número");
            AssertEqual(InputField.Text, model.ActiveField, "Campo inicial tras limpiar");
        }

        private static void Main()
        {
            AcceptsDigitsInNumberField();
            RejectsNonDigitsInNumberField();
            BackspaceAndDeleteRemoveLastCharacter();
            EnterConfirmsCurrentField();
            TabSwitchesBetweenFields();
            TextFieldAcceptsPrintableCharacters();
            ClearResetsBothFields();
            KeyboardByteFormatterTests.Run();
            ZeroReadDoesNotStopReceiving();
            Console.WriteLine("KeyboardInputModelTests: OK");
        }

        private static void ZeroReadDoesNotStopReceiving()
        {
            int calls = 0;
            int waits = 0;
            int received = 0;
            bool running = true;
            UsbReadLoop.Run(
                delegate(byte[] buffer) { calls++; if (calls == 1) return 0; buffer[0] = (byte)'7'; return 1; },
                delegate { return running; },
                delegate { waits++; },
                delegate(byte[] packet) { received = packet[0]; running = false; });
            AssertEqual(1, waits, "Espera tras lectura vacía");
            AssertEqual((int)'7', received, "Lectura posterior a cero");
        }
    }
}
