using System;
using NUnit.Framework;
using ReMi.Commands.Plugins;
using ReMi.CommandValidators.Plugins;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Plugins
{
    public class UpdatePluginGlobalConfigurationEntityCommandValidatorTests : TestClassFor<UpdatePluginGlobalConfigurationEntityCommandValidator>
    {
        protected override UpdatePluginGlobalConfigurationEntityCommandValidator ConstructSystemUnderTest()
        {
            return new UpdatePluginGlobalConfigurationEntityCommandValidator();
        }

        [Test]
        public void Validate_ShouldReturn3Errors_WhenCommandIsEmpty()
        {
            var command = new UpdatePluginGlobalConfigurationEntityCommand();

            var result = Sut.Validate(command);

            Assert.AreEqual(2, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("PluginKey"));
            Assert.True(result.Errors[1].PropertyName.Contains("PropertyName"));
        }

        [Test]
        public void Validate_ShouldReturnError_WhenPluginIdIsEmpytButNotNull()
        {
            var command = new UpdatePluginGlobalConfigurationEntityCommand
            {
                PluginKey = string.Empty,
                JsonValue = string.Empty,
                PropertyName = string.Empty
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(2, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("PluginKey"));
            Assert.True(result.Errors[1].PropertyName.Contains("PropertyName"));
        }

        [Test]
        public void Validate_ShouldNotReturnErrors_WhenCommandIsOk()
        {
            var command = new UpdatePluginGlobalConfigurationEntityCommand
            {
                PluginKey = RandomData.RandomString(10),
                JsonValue = RandomData.RandomString(10),
                PropertyName = RandomData.RandomString(10)
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
