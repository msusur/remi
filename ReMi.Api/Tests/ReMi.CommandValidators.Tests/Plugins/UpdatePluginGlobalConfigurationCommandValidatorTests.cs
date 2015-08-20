using System;
using NUnit.Framework;
using ReMi.Commands.Plugins;
using ReMi.CommandValidators.Plugins;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Plugins
{
    public class UpdatePluginGlobalConfigurationCommandValidatorTests : TestClassFor<UpdatePluginGlobalConfigurationCommandValidator>
    {
        protected override UpdatePluginGlobalConfigurationCommandValidator ConstructSystemUnderTest()
        {
            return new UpdatePluginGlobalConfigurationCommandValidator();
        }

        [Test]
        public void Validate_ShouldReturnOneError_WhenCommandIsEmpty()
        {
            var command = new UpdatePluginGlobalConfigurationCommand();

            var result = Sut.Validate(command);

            Assert.AreEqual(2, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("PluginId"));
            Assert.True(result.Errors[1].PropertyName.Contains("JsonValues"));
        }

        [Test]
        public void Validate_ShouldReturnError_WhenPluginIdIsEmpytButNotNull()
        {
            var command = new UpdatePluginGlobalConfigurationCommand
            {
                PluginId = Guid.NewGuid(),
                JsonValues = ""
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(1, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("JsonValues"));
        }

        [Test]
        public void Validate_ShouldNotReturnErrors_WhenCommandIsOk()
        {
            var command = new UpdatePluginGlobalConfigurationCommand
            {
                PluginId = Guid.NewGuid(),
                JsonValues = "some value"
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
