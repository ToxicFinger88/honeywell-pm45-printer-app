using System;
using Honeywell.Printer;
using PM45DemoV05.Models;

namespace PM45DemoV05.Printing
{
    internal static class LabelBuilder
    {
        public static Drawing Create(string day, int printheadResolution)
        {
            Drawing label = new Drawing();
            label.Height = (int)Math.Round(printheadResolution * 150.0 / 25.4);

            Drawing.Text title = new Drawing.Text();
            title.Point = new Point(100, 700);
            title.Data = LabelText.Title;
            title.Height = 22;
            label += title;

            Drawing.Text selectedDay = new Drawing.Text();
            selectedDay.Point = new Point(100, 520);
            selectedDay.Data = LabelText.Day(day);
            selectedDay.Height = 20;
            label += selectedDay;

            Drawing.Text test = new Drawing.Text();
            test.Point = new Point(100, 330);
            test.Data = LabelText.Test;
            test.Height = 16;
            label += test;

            label.PartialRendering = true;
            return label;
        }
    }
}
