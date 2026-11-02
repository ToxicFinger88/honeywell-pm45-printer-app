using System;

namespace PM45DemoV041.Models
{
    internal sealed class NumericInputBuffer
    {
        private const int MaximumDigits = 3;
        private string value = "";

        public string Value
        {
            get { return value; }
        }

        public void Append(string digit)
        {
            if (digit == null || digit.Length != 1 || digit[0] < '0' || digit[0] > '9')
            {
                return;
            }

            if (value.Length < MaximumDigits)
            {
                value += digit;
            }
        }

        public void Backspace()
        {
            if (value.Length > 0)
            {
                value = value.Substring(0, value.Length - 1);
            }
        }

        public void Set(string initialValue)
        {
            value = initialValue ?? "";
            if (value.Length > MaximumDigits)
            {
                value = value.Substring(0, MaximumDigits);
            }
        }
    }
}
