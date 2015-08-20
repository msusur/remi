using System;
using System.Collections.Generic;

namespace ReMi.Common.Utils
{
    public static class TimeSpanExtensions
    {
        public static string ToDurationString(this TimeSpan duration, string labelSeparator = " ", string partsSeparator = " ")
        {
            if (duration.TotalMilliseconds < 1) return "";

            var result = new List<string>();
            var days = Convert.ToInt32(Math.Floor(duration.TotalDays));
            if (days > 0)
            {
                result.Add(string.Format("{0}{1}d.", days, labelSeparator));
                duration = duration.Subtract(new TimeSpan(days, 0, 0, 0));
            }

            var hs = Convert.ToInt32(Math.Floor(duration.TotalHours));
            if (hs > 0)
            {
                result.Add(string.Format("{0}{1}h.", hs, labelSeparator));
                duration = duration.Subtract(new TimeSpan(0, hs, 0, 0));
            }

            var mins = Convert.ToInt32(Math.Floor(duration.TotalMinutes));
            if (mins > 0)
            {
                result.Add(string.Format("{0}{1}min.", mins, labelSeparator));
                duration = duration.Subtract(new TimeSpan(0, 0, mins, 0));
            }

            var sec = Convert.ToInt32(Math.Floor(duration.TotalSeconds));
            if (sec > 0)
            {
                result.Add(string.Format("{0}{1}s.", sec, labelSeparator));
                duration = duration.Subtract(new TimeSpan(0, 0, 0, sec));
            }

            if (result.Count > 0)
                return string.Join(partsSeparator, result);

            return string.Empty;
        }
    }
}
