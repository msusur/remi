using System.Collections.Generic;
using NUnit.Framework;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Enums;
using ReMi.TestUtils.UnitTests;

namespace ReMi.Common.Utils.Tests.Enums
{
    [TestFixture]
    public class EnumDescriptionHelperTest
    {
        private enum TestEnum
        {
            [EnumOrder(3)]
            Name1 = 1,
            [EnumDescription("Name 2"), EnumOrder(1)]
            Name2,
            [EnumDescription("Name3", Annotation = "Name 3 Annotation"), EnumOrder(2)]
            Name3
        }

        private class TestEnumDescription : OrderedEnumDescription
        {
            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        [Test]
        public void GetEnumDescriptions_ShouldReturnEnumDescriptions_WhenCalled()
        {
            var expected = new List<TestEnumDescription>
            {
                new TestEnumDescription{ Id = 1, Name = "Name1", Description = "Name1", Annotation = null },
                new TestEnumDescription{ Id = 2, Name = "Name2", Description = "Name 2", Annotation = null },
                new TestEnumDescription{ Id = 3, Name = "Name3", Description = "Name3", Annotation = "Name 3 Annotation" }
            };

            var actual = EnumDescriptionHelper.GetEnumDescriptions<TestEnum, TestEnumDescription>();

            CollectionAssertHelper.AreEquivalent(expected, actual);
        }

        [Test]
        public void GetEnumDescription_ShouldReturnEnumDescription_WhenCalled()
        {
            var list = new List<TestEnumDescription>
            {
                new TestEnumDescription{ Id = 1, Name = "Name1", Description = "Name1", Annotation = null },
                new TestEnumDescription{ Id = 2, Name = "Name2", Description = "Name 2", Annotation = null },
                new TestEnumDescription{ Id = 3, Name = "Name3", Description = "Name3", Annotation = "Name 3 Annotation" }
            };

            var actual = EnumDescriptionHelper.GetEnumDescription<TestEnum, TestEnumDescription>(TestEnum.Name2);
            var actual2 = TestEnum.Name2.ToEnumDescription<TestEnum, TestEnumDescription>();

            AssertProperty.AreEqual(list[1], actual);
            AssertProperty.AreEqual(list[1], actual2);
        }

        [Test]
        public void GetDescriptionGeneric_ShouldReturnEnumDescription_WhenCalled()
        {
            var list = new List<TestEnumDescription>
            {
                new TestEnumDescription{ Id = 1, Name = "Name1", Description = "Name1", Annotation = null },
                new TestEnumDescription{ Id = 2, Name = "Name2", Description = "Name 2", Annotation = null },
                new TestEnumDescription{ Id = 3, Name = "Name3", Description = "Name3", Annotation = "Name 3 Annotation" }
            };

            var actual = EnumDescriptionHelper.GetDescription(TestEnum.Name2);
            var actual2 = TestEnum.Name2.ToDescriptionString();

            Assert.AreEqual(list[1].Description, actual);
            Assert.AreEqual(list[1].Description, actual2);
        }

        [Test]
        public void GetDescription_ShouldReturnEnumDescription_WhenCalled()
        {
            var list = new List<TestEnumDescription>
            {
                new TestEnumDescription{ Id = 1, Name = "Name1", Description = "Name1", Annotation = null },
                new TestEnumDescription{ Id = 2, Name = "Name2", Description = "Name 2", Annotation = null },
                new TestEnumDescription{ Id = 3, Name = "Name3", Description = "Name3", Annotation = "Name 3 Annotation" }
            };

            var actual = EnumDescriptionHelper.GetDescription(typeof(TestEnum), TestEnum.Name2);
            var actual2 = TestEnum.Name2.ToDescriptionString();

            Assert.AreEqual(list[1].Description, actual);
            Assert.AreEqual(list[1].Description, actual2);
        }

        [Test]
        public void GetOrderedEnumDescriptions_ShouldReturnOrderEnumDescriptions_WhenCalled()
        {
            var expected = new List<TestEnumDescription>
            {
                new TestEnumDescription{ Id = 1, Name = "Name1", Description = "Name1", Annotation = null, Order = 3},
                new TestEnumDescription{ Id = 2, Name = "Name2", Description = "Name 2", Annotation = null, Order = 1 },
                new TestEnumDescription{ Id = 3, Name = "Name3", Description = "Name3", Annotation = "Name 3 Annotation", Order = 2 }
            };

            var actual = EnumDescriptionHelper.GetOrderedEnumDescriptions<TestEnum, TestEnumDescription>();

            CollectionAssertHelper.AreEquivalent(expected, actual);
        }

        [Test]
        public void GetOrderedEnumDescription_ShouldReturnOrderEnumDescription_WhenCalled()
        {
            var list = new List<TestEnumDescription>
            {
                new TestEnumDescription{ Id = 1, Name = "Name1", Description = "Name1", Annotation = null, Order = 3},
                new TestEnumDescription{ Id = 2, Name = "Name2", Description = "Name 2", Annotation = null, Order = 1 },
                new TestEnumDescription{ Id = 3, Name = "Name3", Description = "Name3", Annotation = "Name 3 Annotation", Order = 2 }
            };

            var actual = EnumDescriptionHelper.GetOrderedEnumDescription<TestEnum, TestEnumDescription>(TestEnum.Name2);
            var actual2 = TestEnum.Name2.ToEnumOrderDescription<TestEnum, TestEnumDescription>();

            AssertProperty.AreEqual(list[1], actual);
            AssertProperty.AreEqual(list[1], actual2);
        }

        [Test]
        public void GetOrder_ShouldReturnOrder_WhenCalled()
        {
            var list = new List<TestEnumDescription>
            {
                new TestEnumDescription{ Id = 1, Name = "Name1", Description = "Name1", Annotation = null, Order = 3},
                new TestEnumDescription{ Id = 2, Name = "Name2", Description = "Name 2", Annotation = null, Order = 1 },
                new TestEnumDescription{ Id = 3, Name = "Name3", Description = "Name3", Annotation = "Name 3 Annotation", Order = 2 }
            };

            var actual = EnumDescriptionHelper.GetOrder(TestEnum.Name2);

            Assert.AreEqual(list[1].Order, actual);
        }
    }
}
