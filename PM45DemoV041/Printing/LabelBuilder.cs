using System;
using Honeywell.Printer;
using PM45DemoV041.Models;

namespace PM45DemoV041.Printing
{
    internal static class LabelBuilder
    {
        public static Drawing Create(PrintSequence sequence, int printheadResolution)
        {
            Drawing label = new Drawing();
            label.Height = (int)Math.Round(printheadResolution * 150.0 / 25.4);

            Drawing.Text title = new Drawing.Text();
            title.Point = new Point(100, 700);
            title.Data = LabelText.Title;
            title.Height = 22;
            label += title;

            Drawing.Text start = new Drawing.Text();
            start.Point = new Point(100, 560);
            start.Data = LabelText.Start(sequence);
            start.Height = 16;
            label += start;

            Drawing.Text end = new Drawing.Text();
            end.Point = new Point(100, 450);
            end.Data = LabelText.End(sequence);
            end.Height = 16;
            label += end;

            Drawing.Text current = new Drawing.Text();
            current.Point = new Point(100, 300);
            current.Data = LabelText.Current(sequence);
            current.Height = 22;
            label += current;

            label.PartialRendering = true;
            return label;
        }
    }
}
