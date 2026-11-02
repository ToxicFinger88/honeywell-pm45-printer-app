namespace PM45DemoV041.Models
{
    internal static class LabelText
    {
        public const string Title = "DEMO VERSION 4.1";

        public static string Start(PrintSequence sequence)
        {
            return "NUMERO DE INICIO: " + sequence.StartNumber.ToString();
        }

        public static string End(PrintSequence sequence)
        {
            return "NUMERO FINAL: " + sequence.EndNumber.ToString();
        }

        public static string Current(PrintSequence sequence)
        {
            return "CORRELATIVO: " + sequence.CurrentNumber.ToString();
        }
    }
}
