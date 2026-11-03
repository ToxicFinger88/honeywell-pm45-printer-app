using System;
using PM45DemoV06.Models;
internal static class DayTypingTests
{
    private static void Main()
    {
        WeekdaySelection days = new WeekdaySelection();
        if (!days.SelectTyped("lunes") || days.SelectedDay != "LUNES") throw new Exception("lunes");
        if (!days.SelectTyped("miercoles") || days.SelectedDay != "MIÉRCOLES") throw new Exception("miercoles");
        if (days.SelectTyped("sabado") || days.SelectTyped("domingo")) throw new Exception("fin de semana");
        if (days.SelectedDay != "MIÉRCOLES") throw new Exception("conservar seleccion");
        Console.WriteLine("DayTypingTests: OK");
    }
}
