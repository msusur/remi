using System;
using NUnit.Framework;
using ReMi.Commands.Auth;
using ReMi.CommandValidators.Auth;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Auth
{
    public class StartSessionCommandValidatorTests : TestClassFor<StartSessionCommandValidator>
    {
        protected override StartSessionCommandValidator ConstructSystemUnderTest()
        {
            return new StartSessionCommandValidator();
        }

        [Test]
        public void Validate_ShouldReturnValidStatis_WhenIdIsNotEmpty()
        {
            var command = new StartSessionCommand
            {
                SessionId = Guid.NewGuid(),
                Login = RandomData.RandomString(10)
            };

            var result = Sut.Validate(command);

            Assert.True(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotReturnValidStatis_WhenIdNotEmpty()
        {
            var command = new StartSessionCommand();

            var result = Sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("'Session Id' should not be empty.", result.Errors[0].ErrorMessage);
            Assert.AreEqual("'Login' should not be empty.", result.Errors[1].ErrorMessage);
        }
    }
}
