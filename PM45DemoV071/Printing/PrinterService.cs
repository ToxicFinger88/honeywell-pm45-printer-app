using System;
using Honeywell.Printer;
using PM45DemoV06.Models;

namespace PM45DemoV06.Printing
{
    internal sealed class PrinterService : IDisposable
    {
        private readonly PrintControl printer;
        private bool disposed;

        public PrinterService() { printer = new PrintControl(); }

        public State Print(PrintSequence sequence, string day)
        {
            if (disposed) throw new ObjectDisposedException("PrinterService");
            State result = State.NoError;
            for (int index = 0; index < sequence.Quantity; index++)
            {
                Drawing label = null;
                try
                {
                    label = LabelBuilder.Create(sequence, day, printer.PrintheadResolution);
                    result = printer.PrintFeed(label, 1);
                    if (result != State.NoError) return result;
                }
                finally
                {
                    if (label != null) label.Dispose();
                }
                sequence.Advance();
            }
            return result;
        }

        public void Dispose()
        {
            if (!disposed) { printer.Dispose(); disposed = true; }
        }
    }
}
