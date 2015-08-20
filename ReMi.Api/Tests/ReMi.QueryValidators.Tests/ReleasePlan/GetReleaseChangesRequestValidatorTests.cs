using System;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryValidators.ReleasePlan;

namespace ReMi.QueryValidators.Tests.ReleasePlan
{
    public class GetReleaseChangesRequestValidatorTests : TestClassFor<GetReleaseChangesRequestValidator>
    {
        protected override GetReleaseChangesRequestValidator ConstructSystemUnderTest()
        {
            return new GetReleaseChangesRequestValidator();
        }

        [Test]
        public void Validate_ShouldBeInvalid_WhenReleaseWindowIdIsEmpty()
        {
            var command = new GetReleaseChangesRequest
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
            var command = new GetReleaseChangesRequest
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
