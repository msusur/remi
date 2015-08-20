using NUnit.Framework;

namespace ReMi.Common.Utils.Tests
{
    [TestFixture]
    public class EqualityComparerTests
    {
        private class TestClass
        {
            public int TestInt { get; set; }
            public string TestString { get; set; }
        }

        [Test]
        [TestCase(5, "five", 5, "five", true, Description = "ShouldReturnTrue_WhenObjectsAreEqual")]
        [TestCase(5, "six", 5, "five", false, Description = "ShouldReturnFalse_WhenStringValueIsNotEqual")]
        [TestCase(5, "five", 6, "five", false, Description = "ShouldReturnFalse_WhenIntValueIsNotEqual")]
        public void Compare_ShouldCompareIntAndStringValue_WhenCalled(int intValue1, string stringValue1, int intValue2, string stringValue2, bool result)
        {
            var sut = new EqualityComparer<TestClass>((t1, t2) => t1.TestInt == t2.TestInt && t1.TestString == t2.TestString);

            var testObj1 = new TestClass
            {
                TestInt = intValue1,
                TestString = stringValue1
            };
            var testObj2 = new TestClass
            {
                TestInt = intValue2,
                TestString = stringValue2
            };

            Assert.AreEqual(result, sut.Equals(testObj1, testObj2));
        }

        [Test]
        [TestCase(5, "five", 5, "five", true, Description = "ShouldReturnTrue_WhenObjectsAreEqual")]
        [TestCase(5, "six", 5, "five", true, Description = "ShouldReturnTrue_WhenIntValuesAreEquelButStringValueIsDifferent")]
        [TestCase(5, "five", 6, "five", false, Description = "ShouldReturnFalse_WhenStringValueIsNotEqual")]
        public void Compare_ShouldCompareOnlyIntValue_WhenCalled(int intValue1, string stringValue1, int intValue2, string stringValue2, bool result)
        {
            var sut = new EqualityComparer<TestClass>((t1, t2) => t1.TestInt == t2.TestInt);

            var testObj1 = new TestClass
            {
                TestInt = intValue1,
                TestString = stringValue1
            };
            var testObj2 = new TestClass
            {
                TestInt = intValue2,
                TestString = stringValue2
            };

            Assert.AreEqual(result, sut.Equals(testObj1, testObj2));
        }
    }
}
