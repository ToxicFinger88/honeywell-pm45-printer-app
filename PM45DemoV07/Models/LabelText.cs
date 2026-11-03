namespace PM45DemoV06.Models
{
    internal static class LabelText
    {
        public const string Title = "DEMO VERSION 7";

        public static string Day(string day) { return "DIA: " + day; }
        public static string Start(PrintSequence sequence) { return "NUMERO DE INICIO: " + sequence.StartNumber; }
        public static string End(PrintSequence sequence) { return "NUMERO FINAL: " + sequence.EndNumber; }
        public static string Current(PrintSequence sequence) { return "CORRELATIVO: " + sequence.CurrentNumber; }
    }
}
