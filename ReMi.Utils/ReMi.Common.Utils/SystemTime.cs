using System;

namespace ReMi.Common.Utils
{
    public static class SystemTime
    {
        private static Func<DateTime> _now = () => DateTime.UtcNow;

        public static DateTime Now
        {
            get { return _now(); }
        }

        public static void Mock(DateTime mockNow)
        {
            _now = () => mockNow;
        }

        public static void Reset()
        {
            _now = () => DateTime.UtcNow;
        }
    }
}
