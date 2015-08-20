using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReMi.TestUtils.UnitTests
{
    public static class RandomData
    {
        private static readonly Random _random = new Random(Guid.NewGuid().GetHashCode());
        private const String _alpha = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //private static String _alphaNumeric = _alpha + "0123456789";

        public static String RandomString(Int32 min, Int32 max)
        {
            var builder = new StringBuilder();
            for (Int32 i = 0; i < _random.Next(min, max); i++)
            {
                builder.Append(_alpha[_random.Next(0, _alpha.Length)]);
            }

            return builder.ToString();
        }

        public static String RandomString(Int32 length)
        {
            return RandomString(length, length);
        }

        public static String RandomEmail()
        {
            return string.Format("{0}@{1}.{2}", RandomString(10), RandomString(5), RandomString(3));
        }

        public static Int32 RandomInt(Int32 min, Int32 max)
        {
            return _random.Next(min, max);
        }

        public static Int32 RandomInt(Int32 max)
        {
            return _random.Next(max);
        }

        public static Int64 RandomLong(Int64 min, Int64 max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException("min", "min is greater than max");
            }

            return min + Convert.ToInt64((max - min) * _random.NextDouble());
        }

        public static Double Random()
        {
            return _random.NextDouble();
        }

        public static DateTime RandomDate()
        {
            return RandomDate(new DateTime(1900, 1, 1), DateTime.Today);
        }

        public static DateTime RandomDate(DateTime min, DateTime max)
        {
            return min.AddDays(_random.Next((max - min).Days)).Date;
        }

        public static DateTime RandomDateTime(DateTimeKind kind = DateTimeKind.Local)
        {
            return RandomDateTime(new DateTime(1900, 1, 1).Ticks, DateTime.Now.Ticks, kind);
        }

        public static DateTime RandomDateTime(long minTicks, long maxTicks, DateTimeKind kind = DateTimeKind.Local)
        {
            return new DateTime((Int64)((maxTicks - minTicks) * _random.NextDouble()), kind);
        }

        public static T RandomEnum<T>()
        {
            return RandomElement((T[])Enum.GetValues(typeof(T)));
        }

        public static T RandomEnum<T>(params T[] exclude)
        {
            return RandomElement(((T[])Enum.GetValues(typeof(T))).Where(t => exclude == null || !exclude.Contains(t)).ToArray());
        }

        public static T RandomEnumOf<T>(params T[] allowed)
        {
            if (allowed == null)
            {
                throw new ArgumentNullException("allowed");
            }

            if (allowed.Length == 0)
            {
                throw new ArgumentOutOfRangeException("allowed", "collection of enum values can not be empty");
            }

            return RandomElement(((T[])Enum.GetValues(typeof(T))).Where(allowed.Contains).ToArray());
        }

        public static bool RandomBool()
        {
            return Convert.ToBoolean(_random.Next(100) % 2);
        }

        public static T RandomElement<T>(IEnumerable<T> enumerable)
        {
            return enumerable.ElementAt(_random.Next(enumerable.Count()));
        }
    }

}
