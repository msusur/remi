using System;
using NUnit.Framework;
using ReMi.Common.Utils.Enums;

namespace ReMi.Common.Utils.Tests.Enums
{
    [TestFixture]
    public class EnumExtensionsTests
    {
        [Flags]
        private enum TestEnum
        {
            Name1 = 1,
            Name2 = 2,
            Name3 = 4
        }

        [Test]
        public void ParseEnum_ShoudReturnParsedEnumType_WhenCalled()
        {
            var actual1 = "Name1".ParseEnum<TestEnum>();
            var actual2 = "name2".ParseEnum<TestEnum>();
            var actual3 = "NAME3".ParseEnum<TestEnum>();

            Assert.AreEqual(TestEnum.Name1, actual1);
            Assert.AreEqual(TestEnum.Name2, actual2);
            Assert.AreEqual(TestEnum.Name3, actual3);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseEnum_ShoudThrowException_WhenCannotParse()
        {
            "Name 1".ParseEnum<TestEnum>();
        }

        [Test]
        public void ToFlagList_ShoudReturnCollectionOfEnumsWhichAreFlagged_WhenCalled()
        {
            const TestEnum testEnum = TestEnum.Name1 | TestEnum.Name3;
            var actual = testEnum.ToFlagList();

            CollectionAssert.AreEquivalent(new[] { TestEnum.Name1, TestEnum.Name3 }, actual);
        }

    }
}
