using Moq;
using NUnit.Framework;
using ReMi.Common.Utils;
using ReMi.Plugin.Common.PluginsConfiguration;
using System.Linq;

namespace ReMi.Plugin.Common.Tests.PluginsConfiguration
{
    [TestFixture]
    public class PropertyUpdaterTests
    {
        private enum TestEnum
        {
            TestEnum1,
            TestEnum2
        }
        private class TestClass
        {
            public TestEnum TestEnum { get; set; }
            public string TestString { get; set; }
            public int TestInt { get; set; }
            public int[] TestObject { get; set; }
        }

        private Mock<ISerialization> _serializationMock;

        [SetUp]
        public void TestSetUp()
        {
            _serializationMock = new Mock<ISerialization>(MockBehavior.Strict);
        }

        [Test]
        [TestCase("TestEnum", TestEnum.TestEnum2, Description = "UpdatePropertyValue_ShouldUpdateTestEnumProperty_WhenCalled")]
        [TestCase("TestString", "string test", Description = "UpdatePropertyValue_ShouldUpdateTestStringProperty_WhenCalled")]
        [TestCase("TestInt", 150, Description = "UpdatePropertyValue_ShouldUpdateTestIntProperty_WhenCalled")]
        [TestCase("TestObject",  new[] { 1, 2, 3, 4 }, Description = "UpdatePropertyValue_ShouldUpdateTestObjectProperty_WhenCalled")]
        public void UpdatePropertyValue_ShouldUpdateValue_WhenPropertyFound(string propertyName, object value)
        {
            var testClass = new TestClass();
            var propertyInfo = typeof(TestClass).GetProperties()
                .First(x => x.Name == propertyName);
            testClass.UpdatePropertyValue(propertyName, value);

            Assert.AreEqual(value, propertyInfo.GetValue(testClass));
        }

        [Test]
        [TestCase("TestEnum", "TestEnum2", TestEnum.TestEnum2, Description = "UpdatePropertyValue_ShouldDeserializeAndUpdateTestEnumProperty_WhenCalled")]
        [TestCase("TestString", "string test", "string test", Description = "UpdatePropertyValue_ShouldDeserializeAndUpdateTestStringProperty_WhenCalled")]
        [TestCase("TestInt", "150", 150, Description = "UpdatePropertyValue_ShouldDeserializeAndUpdateTestIntProperty_WhenCalled")]
        [TestCase("TestObject", "[1,2,3,4]", new[] { 1, 2, 3, 4 }, Description = "UpdatePropertyValue_ShouldDeserializeAndUpdateTestObjectProperty_WhenCalled")]
        public void UpdatePropertyValue_ShouldDeserializeAndUpdateValue_WhenPropertyFound(string propertyName, object value, object expected)
        {
            _serializationMock.Setup(x => x.FromJson(value.ToString(), expected.GetType()))
                .Returns(expected);

            var testClass = new TestClass();
            var propertyInfo = typeof(TestClass).GetProperties()
                .First(x => x.Name == propertyName);
            testClass.UpdatePropertyValue(propertyName, value, _serializationMock.Object);

            Assert.AreEqual(expected, propertyInfo.GetValue(testClass));
        }
    }
}
