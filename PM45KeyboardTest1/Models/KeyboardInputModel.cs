using System;

namespace PM45KeyboardTest1.Models
{
    internal enum InputField
    {
        Text,
        Number
    }

    internal sealed class KeyboardInputModel
    {
        private string textValue = "";
        private string numberValue = "";

        public InputField ActiveField { get; private set; }

        public string TextValue
        {
            get { return textValue; }
        }

        public string NumberValue
        {
            get { return numberValue; }
        }

        public string LastStatus { get; private set; }

        public KeyboardInputModel()
        {
            ActiveField = InputField.Text;
            LastStatus = "Campo seleccionado: TEXTO";
        }

        public void SelectField(InputField field)
        {
            ActiveField = field;
            LastStatus = "Campo seleccionado: " + FieldName(field);
        }

        public void ProcessByte(byte value)
        {
            if (value == 9)
            {
                SelectField(ActiveField == InputField.Text ? InputField.Number : InputField.Text);
                return;
            }

            if (value == 8 || value == 127)
            {
                RemoveLastCharacter();
                return;
            }

            if (value == 10 || value == 13)
            {
                LastStatus = "Campo confirmado: " + FieldName(ActiveField) + " = " + CurrentValue();
                return;
            }

            if (value < 32 || value > 126)
            {
                LastStatus = "Tecla ignorada: 0x" + value.ToString("X2");
                return;
            }

            char character = (char)value;
            if (ActiveField == InputField.Number)
            {
                if (character < '0' || character > '9')
                {
                    LastStatus = "Dato inválido en NUMERO: " + character;
                    return;
                }

                numberValue += character;
                LastStatus = "Valor introducido en NUMERO: " + numberValue;
                return;
            }

            textValue += character;
            LastStatus = "Valor introducido en TEXTO: " + textValue;
        }

        public void Clear()
        {
            textValue = "";
            numberValue = "";
            ActiveField = InputField.Text;
            LastStatus = "Campos limpiados; campo seleccionado: TEXTO";
        }

        private void RemoveLastCharacter()
        {
            string value = CurrentValue();
            if (value.Length == 0)
            {
                LastStatus = "No hay caracteres para borrar en " + FieldName(ActiveField);
                return;
            }

            value = value.Substring(0, value.Length - 1);
            if (ActiveField == InputField.Text) textValue = value;
            else numberValue = value;
            LastStatus = "Último carácter borrado en " + FieldName(ActiveField);
        }

        private string CurrentValue()
        {
            return ActiveField == InputField.Text ? textValue : numberValue;
        }

        private static string FieldName(InputField field)
        {
            return field == InputField.Text ? "TEXTO" : "NUMERO";
        }
    }
}
