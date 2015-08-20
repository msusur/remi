using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ReMi.TestUtils.UnitTests
{
    public static class CollectionAssertHelper
    {
        public static void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, Func<T, T, bool> compareMethod)
        {
            AreEqual<T, T>(expected, actual, compareMethod);
        }

        public static void AreEqual<T1, T2>(IEnumerable<T1> expected, IEnumerable<T2> actual, Func<T1, T2, bool> compareMethod)
        {
            if (compareMethod == null)
            {
                throw new ArgumentNullException("compareMethod");
            }

            if (expected == null || actual == null)
            {
                Assert.IsNull(expected);
                Assert.IsNull(actual);
                return;
            }

            var expectedCount = expected.Count();
            var actualCount = actual.Count();
            Assert.AreEqual(expectedCount, actualCount,
                string.Format("Collections have diffent amount of elements: expected {0}, actual {1}", expectedCount, actualCount));

            var expectedEnumerator = expected.GetEnumerator();
            var actulaEnumerator = actual.GetEnumerator();

            var index = 0;
            while (expectedEnumerator.MoveNext() && actulaEnumerator.MoveNext())
            {
                Assert.IsTrue(compareMethod(expectedEnumerator.Current, actulaEnumerator.Current),
                    string.Format("Elements at index [{0}] are different", index));
                index++;
            }
        }

        public static void AreEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual)
            where T : class
        {
            if (expected == null || actual == null)
            {
                Assert.IsNull(expected);
                Assert.IsNull(actual);
                return;
            }

            var expectedCount = expected.Count();
            var actualCount = actual.Count();
            Assert.AreEqual(expectedCount, actualCount,
                string.Format("Collections have diffent amount of elements: expected {0}, actual {1}", expectedCount, actualCount));

            if (expectedCount == 0 && actualCount == 0)
                return;

            var expectedEnumerator = expected.GetEnumerator();
            var actulaEnumerator = actual.GetEnumerator();

            while (expectedEnumerator.MoveNext() && actulaEnumerator.MoveNext())
            {
                var type = expectedEnumerator.Current.GetType();
                if (type.IsClass && type != typeof(string))
                    AssertProperty.AreEqual(expectedEnumerator.Current, actulaEnumerator.Current);
                else
                    Assert.AreEqual(expectedEnumerator.Current, actulaEnumerator.Current);
            }

        }
    }
}
