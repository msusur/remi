using NUnit.Framework;
using ReMi.Commands.Configuration;
using ReMi.CommandValidators.Configuration;
using System;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Configuration
{
    public class RemoveBusinessUnitCommandValidatorTests : TestClassFor<RemoveBusinessUnitCommandValidator>
    {
        protected override RemoveBusinessUnitCommandValidator ConstructSystemUnderTest()
        {
            return new RemoveBusinessUnitCommandValidator();
        }

        [Test]
        public void Validate_ShouldReturnThreeError_WhenCommandIsEmpty()
        {
            var command = new RemoveBusinessUnitCommand();

            var result = Sut.Validate(command);

            Assert.AreEqual(1, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("ExternalId"));
        }


        [Test]
        public void Validate_ShouldNotReturnErrors_WhenCommandIsOk()
        {
            var command = new RemoveBusinessUnitCommand
            {
                ExternalId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
