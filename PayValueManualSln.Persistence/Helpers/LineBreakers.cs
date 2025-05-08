namespace PayValueManualSln.Infrastructure.Persistence.Helpers
{
    public static class LineBreakers
    {
        public static string ShowLineBreaks(object text)
        {
            return (text.ToString().Replace("\n", "<br/>"));
        }
    }
}
