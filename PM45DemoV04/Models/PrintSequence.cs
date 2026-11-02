using System;

namespace PM45DemoV04.Models
{
    internal sealed class PrintSequence
    {
        private const int Minimum = 1;
        private const int Maximum = 999;

        public PrintSequence(int startNumber, int quantity)
        {
            Validate(startNumber, "numero de inicio");
            Validate(quantity, "cantidad");

            StartNumber = startNumber;
            Quantity = quantity;
            EndNumber = AdvanceNumber(startNumber, quantity - 1);
            CurrentNumber = startNumber;
        }

        public int StartNumber { get; private set; }
        public int Quantity { get; private set; }
        public int EndNumber { get; private set; }
        public int CurrentNumber { get; private set; }

        public void Advance()
        {
            CurrentNumber = AdvanceNumber(CurrentNumber, 1);
        }

        private static void Validate(int value, string name)
        {
            if (value < Minimum || value > Maximum)
            {
                throw new ArgumentOutOfRangeException(name, "El valor debe estar entre 1 y 999.");
            }
        }

        private static int AdvanceNumber(int value, int amount)
        {
            int zeroBased = (value - Minimum + amount) % Maximum;
            if (zeroBased < 0)
            {
                zeroBased += Maximum;
            }

            return zeroBased + Minimum;
        }
    }
}
