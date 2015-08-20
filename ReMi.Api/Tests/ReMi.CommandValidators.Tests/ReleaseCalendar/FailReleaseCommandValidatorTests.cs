using NUnit.Framework;
using ReMi.Commands.ReleaseCalendar;
using ReMi.CommandValidators.ReleaseCalendar;
using System;
using System.Linq;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleaseCalendar
{
    [TestFixture]
    public class FailReleaseCommandValidatorTests : TestClassFor<FailReleaseCommandValidator>
    {
        protected override FailReleaseCommandValidator ConstructSystemUnderTest()
        {
            return new FailReleaseCommandValidator();
        }

        [Test]
        public void Validate_ShouldPass_WhenCommandValid()
        {
            var command = new FailReleaseCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10)
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenPropertiesAreEmpty()
        {
            var command = new FailReleaseCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                UserName = string.Empty,
                Password = string.Empty
            };


            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("UserName", result.Errors.ElementAt(0).PropertyName);
            Assert.AreEqual("Password", result.Errors.ElementAt(1).PropertyName);
        }
    }
}
