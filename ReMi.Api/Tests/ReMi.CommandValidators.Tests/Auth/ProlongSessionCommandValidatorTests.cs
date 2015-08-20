using System;
using NUnit.Framework;
using ReMi.Commands.Auth;
using ReMi.CommandValidators.Auth;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Auth
{
    public class ProlongSessionCommandValidatorTests : TestClassFor<ProlongSessionCommandValidator>
    {
        protected override ProlongSessionCommandValidator ConstructSystemUnderTest()
        {
            return new ProlongSessionCommandValidator();
        }

        [Test]
        public void Validate_ShouldReturnValidStatis_WhenIdIsNotEmpty()
        {
            var command = new ProlongSessionCommand
            {
                SessionId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.True(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotReturnValidStatis_WhenIdNotEmpty()
        {
            var command = new ProlongSessionCommand();

            var result = Sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Session Id cannot be empty", result.Errors[0].ErrorMessage);
        }
    }
}
