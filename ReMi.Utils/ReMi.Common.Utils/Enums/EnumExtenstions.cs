using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.Common.Utils.Enums
{
    public static class EnumParser
    {
        public static T ParseEnum<T>(this string value)
            where T : struct, IConvertible, IComparable, IFormattable
        {
            return (T) Enum.Parse(typeof (T), value, true);
        }

        public static IEnumerable<T> ToFlagList<T>(this T value)
            where T : struct, IConvertible, IComparable, IFormattable
        {
            var enumValue = value as Enum;
            return enumValue == null
                ? null
                : Enum.GetValues(value.GetType()).Cast<Enum>().Where(enumValue.HasFlag).Cast<T>();
        }
    }
}
