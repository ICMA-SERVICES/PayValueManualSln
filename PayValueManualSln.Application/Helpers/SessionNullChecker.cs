using System;

namespace PayValueManualSln.Application.Helpers
{
    public static class SessionNullChecker
    {
        public static string ObjectToString(this object value)
        {
            return value == null ? null : Convert.ToString(value);

            //if (value == null) return null;
            //return Convert.ToString(value);
        }
    }

    public static class SessionNullCheckers
    {
        public static string ObjectToStrings(this object value)
        {
            if (value == null) return null;
            return Convert.ToString(value);
        }
    }
}
