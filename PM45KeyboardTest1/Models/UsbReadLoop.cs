using System;

namespace PM45KeyboardTest1.Models
{
    internal static class UsbReadLoop
    {
        public static void Run(Func<byte[], int> read, Func<bool> running, Action wait, Action<byte[]> receive)
        {
            byte[] buffer = new byte[256];
            while (running())
            {
                int count = read(buffer);
                if (count == 0)
                {
                    wait();
                    continue;
                }
                if (count < 0 || count > buffer.Length) throw new InvalidOperationException("Lectura USB inválida");
                byte[] packet = new byte[count];
                Array.Copy(buffer, packet, count);
                receive(packet);
            }
        }
    }
}
