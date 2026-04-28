namespace HRManagement.Helpers
{
    public static class CleanText
    {
        public static string CleanTextFunction(string text)
        {
            return text
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Trim();
        }
    }
}
