using System;
using System.Linq;
using NUnit.Framework;
using ReMi.Commands.ReleasePlan;
using ReMi.CommandValidators.ReleasePlan;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleasePlan
{
    public class ReapproveTicketsCommandValidatorTests : TestClassFor<ReapproveTicketsCommandValidator>
    {
        protected override ReapproveTicketsCommandValidator ConstructSystemUnderTest()
        {
            return new ReapproveTicketsCommandValidator();
        }

        [Test]
        public void Validate_ShouldPass_WhenReleaseWindowIdIsNotEmpty()
        {
            var command = new ReapproveTicketsCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenReleaseWindowIdIsEmpty()
        {
            var command = new ReapproveTicketsCommand
            {
                ReleaseWindowId = Guid.Empty
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("ReleaseWindowId", result.Errors.ElementAt(0).PropertyName);
        }
    }
}
