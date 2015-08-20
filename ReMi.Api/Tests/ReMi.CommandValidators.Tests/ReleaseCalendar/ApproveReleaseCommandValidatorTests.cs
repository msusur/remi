using NUnit.Framework;
using ReMi.Commands.ReleaseCalendar;
using ReMi.CommandValidators.ReleaseCalendar;
using System;
using System.Linq;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleaseCalendar
{
    [TestFixture]
    public class ApproveReleaseCommandValidatorTests : TestClassFor<ApproveReleaseCommandValidator>
    {
        protected override ApproveReleaseCommandValidator ConstructSystemUnderTest()
        {
            return new ApproveReleaseCommandValidator();
        }

        [Test]
        public void Validate_ShouldPass_WhenCommandValid()
        {
            var command = new ApproveReleaseCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10)
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenPropertiesAreEmpty()
        {
            var command = new ApproveReleaseCommand
            {
                ReleaseWindowId = Guid.Empty,
                AccountId = Guid.Empty,
                UserName = string.Empty,
                Password = string.Empty
            };


            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(4, result.Errors.Count);
            Assert.AreEqual("ReleaseWindowId", result.Errors.ElementAt(0).PropertyName);
            Assert.AreEqual("AccountId", result.Errors.ElementAt(1).PropertyName);
            Assert.AreEqual("UserName", result.Errors.ElementAt(2).PropertyName);
            Assert.AreEqual("Password", result.Errors.ElementAt(3).PropertyName);
        }
    }
}
