using System;
using NUnit.Framework;
using ReMi.Commands.ReleasePlan;
using ReMi.CommandValidators.ReleasePlan;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleasePlan
{
    public class ReAssignReleaseChangesToReleaseCommandValidatorTests : TestClassFor<ReAssignReleaseChangesToReleaseCommandValidator>
    {
        protected override ReAssignReleaseChangesToReleaseCommandValidator ConstructSystemUnderTest()
        {
            return new ReAssignReleaseChangesToReleaseCommandValidator();
        }

        [Test]
        public void Validate_ShouldBeInvalid_WhenReleaseWindowIdIsEmpty()
        {
            var command = new ReAssignReleaseChangesToReleaseCommand
            {
                ReleaseWindowId = Guid.Empty
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(1, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("ReleaseWindowId"));
        }


        [Test]
        public void Validate_ShouldBeValid_WhenCommandIsCorrect()
        {
            var command = new ReAssignReleaseChangesToReleaseCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
