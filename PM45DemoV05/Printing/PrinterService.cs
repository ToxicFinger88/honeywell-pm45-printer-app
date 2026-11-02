using System;
using Honeywell.Printer;

namespace PM45DemoV05.Printing
{
    internal sealed class PrinterService : IDisposable
    {
        private readonly PrintControl printer;
        private bool disposed;

        public PrinterService()
        {
            printer = new PrintControl();
        }

        public State Print(string day)
        {
            EnsureNotDisposed();
            Drawing label = null;
            try
            {
                label = LabelBuilder.Create(day, printer.PrintheadResolution);
                return printer.PrintFeed(label, 1);
            }
            finally
            {
                if (label != null)
                {
                    label.Dispose();
                }
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                printer.Dispose();
                disposed = true;
            }
        }

        private void EnsureNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PrinterService");
            }
        }
    }
}
