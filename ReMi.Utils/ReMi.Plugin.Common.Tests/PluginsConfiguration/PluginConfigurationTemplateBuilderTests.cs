using AutoMapper.Internal;
using NUnit.Framework;
using ReMi.Contracts.Enums;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Common.PluginsConfiguration;
using System.Linq;
using ReMi.TestUtils.UnitTests;

namespace ReMi.Plugin.Common.Tests.PluginsConfiguration
{
    [TestFixture]
    public class PluginConfigurationTemplateBuilderTests
    {
        private enum TestEnum
        {
            Test1,
            Test2,
            [EnumDescription("Test 3")]
            Test3
        }

        private class TestConfigurationEntity : IPluginConfigurationEntity
        {
            public string TestProperty1 { get; set; }

            [PluginConfiguration("Test property 2", PluginConfigurationType.Int)]
            public int TestProperty2 { get; set; }
            [PluginConfiguration("Test property 3", PluginConfigurationType.String)]
            public string TestProperty3 { get; set; }
            [PluginConfiguration("Test property 4", PluginConfigurationType.Enum)]
            public TestEnum TestProperty4 { get; set; }
            [PluginConfiguration("Test property 5", PluginConfigurationType.Password)]
            public string TestProperty5 { get; set; }
            [PluginConfiguration("Test property 6", PluginConfigurationType.NameValueCollection)]
            public IDictionary<string, string> TestProperty6 { get; set; }
            [PluginConfiguration("Test property 7", PluginConfigurationType.Json)]
            public IDictionary<string, string> TestProperty7 { get; set; }
            [PluginConfiguration("Test property 8", PluginConfigurationType.Select)]
            public IDictionary<string, string> TestProperty8 { get; set; }
        }

        [Test]
        public void Build_ShouldBuildConfigurationTemplateOfConfigurationClass_WhenCalled()
        {

            var result = PluginConfigurationTemplateBuilder.Build<TestConfigurationEntity>(
                () => new[] { new PluginConfigurationSelectTemplate { Description = "Description", Value = "Value" } })
                    .ToArray();

            Assert.AreEqual(7, result.Count(), "Incorrect amount of configurable properties");
            Assert.AreEqual(1, result.Count(x => x.Enums != null), "Incorrect amount of enum configurable properties");
            AssertProperty.AreEqual(
                new PluginConfigurationTemplate { PropertyName = "TestProperty2", Description = "Test property 2", Enums = null, Type = PluginConfigurationType.Int },
                result.First(x => x.PropertyName == "TestProperty2"));
            AssertProperty.AreEqual(
                new PluginConfigurationTemplate { PropertyName = "TestProperty3", Description = "Test property 3", Enums = null, Type = PluginConfigurationType.String },
                result.First(x => x.PropertyName == "TestProperty3"));
            AssertProperty.AreEqual(
                new PluginConfigurationTemplate
                {
                    PropertyName = "TestProperty4",
                    Description = "Test property 4",
                    Enums = new[]
                    {
                        new PluginConfigurationEnumTemplate{Description = "Test1", Name = "Test1" },
                        new PluginConfigurationEnumTemplate{Description = "Test2", Name = "Test2" },
                        new PluginConfigurationEnumTemplate{Description = "Test 3", Name = "Test3" }
                    },
                    Type = PluginConfigurationType.Enum
                },
                result.First(x => x.PropertyName == "TestProperty4"));
            AssertProperty.AreEqual(
                new PluginConfigurationTemplate { PropertyName = "TestProperty5", Description = "Test property 5", Enums = null, Type = PluginConfigurationType.Password },
                result.First(x => x.PropertyName == "TestProperty5"));
            AssertProperty.AreEqual(
                new PluginConfigurationTemplate { PropertyName = "TestProperty6", Description = "Test property 6", Enums = null, Type = PluginConfigurationType.NameValueCollection },
                result.First(x => x.PropertyName == "TestProperty6"));
            AssertProperty.AreEqual(
                new PluginConfigurationTemplate { PropertyName = "TestProperty7", Description = "Test property 7", Enums = null, Type = PluginConfigurationType.Json },
                result.First(x => x.PropertyName == "TestProperty7"));
            AssertProperty.AreEqual(
                new PluginConfigurationTemplate
                {
                    PropertyName = "TestProperty8",
                    Description = "Test property 8",
                    Enums = null,
                    Type = PluginConfigurationType.Select,
                    Select = new[] { new PluginConfigurationSelectTemplate { Description = "Description", Value = "Value" } }
                },
                result.First(x => x.PropertyName == "TestProperty8"));
        }
    }
}
