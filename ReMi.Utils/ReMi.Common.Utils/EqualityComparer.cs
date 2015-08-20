using System;
using System.Collections.Generic;

namespace ReMi.Common.Utils
{
    public class EqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _compare;
        public EqualityComparer(Func<T, T, bool> compare)
        {
            _compare = compare;
        }

        public bool Equals(T x, T y)
        {
            return _compare(x, y);
        }

        public int GetHashCode(T obj)
        {
            return 0;
        }

        public static IEqualityComparer<T> Compare(Func<T, T, bool> compare)
        {
            return new EqualityComparer<T>(compare);
        }
    }
}
