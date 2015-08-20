using System.Collections.Generic;
using NUnit.Framework;

namespace ReMi.TestUtils.UnitTests
{
    public static class AssertProperty
    {
        private const string Message = "Expected property [{0}] value not equal to actual value";

        public static void AreEqual<T>(T expected, T actual)
            where T : class
        {
            if (expected == null || actual == null)
            {
                Assert.IsNull(expected);
                Assert.IsNull(actual);
                return;
            }
            var type = expected.GetType() == actual.GetType() ? expected.GetType() : typeof(T);

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var expectedValue = property.GetValue(expected);
                var actualValue = property.GetValue(actual);
                if (expectedValue is IEnumerable<object> && actualValue is IEnumerable<object>)
                {
                    CollectionAssertHelper.AreEquivalent((IEnumerable<object>)expectedValue, (IEnumerable<object>)actualValue);
                }
                else
                {
                    Assert.AreEqual(expectedValue, actualValue, Message, property.Name);
                }
            }
        }
    }
}
