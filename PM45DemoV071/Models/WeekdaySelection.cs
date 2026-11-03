using System.Collections.Generic;
using System;

namespace PM45DemoV06.Models
{
    internal sealed class WeekdaySelection
    {
        private readonly List<string> days = new List<string>
        {
            "LUNES",
            "MARTES",
            "MIÉRCOLES",
            "JUEVES",
            "VIERNES"
        };

        private string selectedDay = "";

        public IList<string> Days
        {
            get { return days.AsReadOnly(); }
        }

        public string SelectedDay
        {
            get { return selectedDay; }
        }

        public void Select(string day)
        {
            if (day != null && days.Contains(day))
            {
                selectedDay = day;
            }
        }

        public void Clear()
        {
            selectedDay = "";
        }

        public bool Confirm()
        {
            return selectedDay.Length > 0;
        }

        public bool SelectTyped(string value)
        {
            string text = (value ?? "").Trim().ToUpperInvariant();
            if (text == "MIERCOLES") text = "MIÉRCOLES";
            if (!days.Contains(text)) return false;
            selectedDay = text;
            return true;
        }
    }
}
