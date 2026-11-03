using System;
using PM45KeyboardTest1.Models;

namespace PM45KeyboardTest1.Tests
{
    internal static class KeyboardByteFormatterTests
    {
        private static void AssertEqual(string expected, string actual, string message)
        {
            if (expected != actual)
            {
                throw new Exception(message + ": esperado '" + expected + "', obtenido '" + actual + "'");
            }
        }

        private static void DescribesPrintableAndControlBytes()
        {
            AssertEqual("DEC=65 HEX=0x41 CHAR='A'", KeyboardByteFormatter.Describe((byte)'A'), "Byte imprimible");
            AssertEqual("DEC=8 HEX=0x08 KEY=BACKSPACE", KeyboardByteFormatter.Describe(8), "Backspace");
            AssertEqual("DEC=127 HEX=0x7F KEY=DELETE", KeyboardByteFormatter.Describe(127), "Delete");
            AssertEqual("DEC=9 HEX=0x09 KEY=TAB", KeyboardByteFormatter.Describe(9), "Tab");
            AssertEqual("DEC=13 HEX=0x0D KEY=ENTER", KeyboardByteFormatter.Describe(13), "Enter");
        }

        internal static void Run()
        {
            DescribesPrintableAndControlBytes();
        }
    }
}
