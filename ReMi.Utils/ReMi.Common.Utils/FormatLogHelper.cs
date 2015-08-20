namespace ReMi.Common.Utils
{
    public static class FormatLogHelper
    {
        public static string FormatEntry(ISerialization serialization, IApplicationSettings applicationSettings, object obj)
        {
            if (obj == null)
                return null;

            return serialization.ToJson(obj, new[] { "Password" }, applicationSettings.LogJsonFormatted);
        }
    }
}
