using System.Collections.Generic;

namespace PM45DemoV05.Models
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
    }
}
