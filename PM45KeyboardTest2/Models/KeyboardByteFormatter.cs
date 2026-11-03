namespace PM45KeyboardTest1.Models
{
    internal static class KeyboardByteFormatter
    {
        public static string Describe(byte value)
        {
            if (value == 8) return "DEC=8 HEX=0x08 KEY=BACKSPACE";
            if (value == 127) return "DEC=127 HEX=0x7F KEY=DELETE";
            if (value == 9) return "DEC=9 HEX=0x09 KEY=TAB";
            if (value == 10 || value == 13) return "DEC=" + value + " HEX=0x" + value.ToString("X2") + " KEY=ENTER";
            if (value >= 32 && value <= 126) return "DEC=" + value + " HEX=0x" + value.ToString("X2") + " CHAR='" + (char)value + "'";
            return "DEC=" + value + " HEX=0x" + value.ToString("X2") + " KEY=CONTROL";
        }
    }
}
