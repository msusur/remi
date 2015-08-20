using System;
using NUnit.Framework;
using ReMi.Commands.Plugins;
using ReMi.CommandValidators.Plugins;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Plugins
{
    public class UpdatePluginPackageConfigurationCommandValidatorTests : TestClassFor<UpdatePluginPackageConfigurationCommandValidator>
    {
        protected override UpdatePluginPackageConfigurationCommandValidator ConstructSystemUnderTest()
        {
            return new UpdatePluginPackageConfigurationCommandValidator();
        }

        [Test]
        public void Validate_ShouldReturnError_WhenCommandIsEmpty()
        {
            var command = new UpdatePluginPackageConfigurationCommand();

            var result = Sut.Validate(command);

            Assert.AreEqual(3, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("PluginId"));
            Assert.True(result.Errors[1].PropertyName.Contains("PackageId"));
            Assert.True(result.Errors[2].PropertyName.Contains("JsonValues"));
        }

        [Test]
        public void Validate_ShouldReturnError_WhenPackageIdAndValuesAreEmpty()
        {
            var command = new UpdatePluginPackageConfigurationCommand
            {
                PluginId = Guid.NewGuid(),
                PackageId = Guid.Empty,
                JsonValues = ""
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(2, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("PackageId"));
            Assert.True(result.Errors[1].PropertyName.Contains("JsonValues"));
        }

        [Test]
        public void Validate_ShouldNotReturnErrors_WhenCommandIsOk()
        {
            var command = new UpdatePluginPackageConfigurationCommand
            {
                PluginId = Guid.NewGuid(),
                PackageId = Guid.NewGuid(),
                JsonValues = "some value"
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
