using System;
using System.Data.SqlTypes;

namespace ReMi.Common.Utils
{
    public static class DateTimeExtensions
    {
        public static DateTime? ToMssqlRangeDateTime(this DateTime? datetime)
        {
            if (!datetime.HasValue)
                return null;

            if (datetime.Value < SqlDateTime.MinValue.Value || datetime.Value > SqlDateTime.MaxValue.Value)
                return null;

            return datetime.Value;
        }

        public static DateTime ToMssqlRangeDateTime(this DateTime datetime, DateTime defaultValue)
        {
            var fixedDate = ToMssqlRangeDateTime(datetime);

            if (!fixedDate.HasValue)
                return defaultValue;

            return fixedDate.Value;
        }
    }
}
