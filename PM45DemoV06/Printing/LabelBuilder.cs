using System;
using Honeywell.Printer;
using PM45DemoV06.Models;

namespace PM45DemoV06.Printing
{
    internal static class LabelBuilder
    {
        public static Drawing Create(PrintSequence sequence, string day, int resolution)
        {
            Drawing label = new Drawing();
            label.Height = (int)Math.Round(resolution * 150.0 / 25.4);
            AddText(label, 100, 730, LabelText.Title, 22);
            AddText(label, 100, 600, LabelText.Day(day), 18);
            AddText(label, 100, 500, LabelText.Start(sequence), 16);
            AddText(label, 100, 410, LabelText.End(sequence), 16);
            AddText(label, 100, 280, LabelText.Current(sequence), 20);
            label.PartialRendering = true;
            return label;
        }

        private static void AddText(Drawing label, int x, int y, string data, int height)
        {
            Drawing.Text text = new Drawing.Text();
            text.Point = new Point(x, y);
            text.Data = data;
            text.Height = height;
            label += text;
        }
    }
}
