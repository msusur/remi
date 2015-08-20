using NUnit.Framework;
using ReMi.Commands.SourceControl;
using ReMi.CommandValidators.SourceControl;
using ReMi.TestUtils.UnitTests;
using System;

namespace ReMi.CommandValidators.Tests.ReleasePlan
{
    public class StoreSourceControlChangesCommandValidatorTests : TestClassFor<StoreSourceControlChangesCommandValidator>
    {
        protected override StoreSourceControlChangesCommandValidator ConstructSystemUnderTest()
        {
            return new StoreSourceControlChangesCommandValidator();
        }

        [Test]
        public void Validate_ShouldBeInvalid_WhenReleaseWindowIdIsEmpty()
        {
            var command = new StoreSourceControlChangesCommand
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
            var command = new StoreSourceControlChangesCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
