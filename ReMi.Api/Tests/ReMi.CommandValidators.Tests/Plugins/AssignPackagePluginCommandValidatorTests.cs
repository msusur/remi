using System;
using NUnit.Framework;
using ReMi.Commands.Plugins;
using ReMi.CommandValidators.Plugins;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Plugins
{
    public class AssignPackagePluginCommandValidatorTests : TestClassFor<AssignPackagePluginCommandValidator>
    {
        protected override AssignPackagePluginCommandValidator ConstructSystemUnderTest()
        {
            return new AssignPackagePluginCommandValidator();
        }

        [Test]
        public void Validate_ShouldReturnOneError_WhenCommandIsEmpty()
        {
            var command = new AssignPackagePluginCommand();

            var result = Sut.Validate(command);

            Assert.AreEqual(1, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("ConfigurationId"));
        }

        [Test]
        public void Validate_ShouldReturnError_WhenPluginIdIsEmpytButNotNull()
        {
            var command = new AssignPackagePluginCommand
            {
                ConfigurationId = Guid.NewGuid(),
                PluginId = Guid.Empty
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(1, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("PluginId"));
        }

        [Test]
        public void Validate_ShouldNotReturnErrors_WhenCommandIsOk()
        {
            var command = new AssignPackagePluginCommand
            {
                ConfigurationId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
