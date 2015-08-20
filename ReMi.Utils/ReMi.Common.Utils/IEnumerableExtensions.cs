using System.Collections.Generic;
using System.Linq;

namespace ReMi.Common.Utils
{
    public static class IEnumerableExtensions
    {
        public static string FormatElements<T>(this IEnumerable<T> values, string startBound, string endBound)
        {
            var s = startBound;
            if (string.IsNullOrWhiteSpace(s))
                s = string.Empty;

            var e = endBound;
            if (string.IsNullOrWhiteSpace(e))
                e = string.Empty;

            if (values == null) return s + e;

            return string.Format("{0}{1}{2}", s, string.Join(", ", values), e);
        }

        public static string FormatElements<T>(this IEnumerable<T> values)
        {
            return FormatElements(values, "[", "]");
        }


        public static string FormatElements<T>(this List<T> values)
        {
            return FormatElements<T>(values.AsEnumerable());
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> values)
        {
            return (values == null || !values.Any());
        }
    }
}
