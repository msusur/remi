using System;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.Commands.ReleasePlan;
using ReMi.CommandValidators.ReleasePlan;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleasePlan
{
    public class UpdateReleaseJobCommandValidatorTests : TestClassFor<UpdateReleaseJobCommandValidator>
    {
        protected override UpdateReleaseJobCommandValidator ConstructSystemUnderTest()
        {
            return new UpdateReleaseJobCommandValidator();
        }

        [Test]
        public void Validate_ShouldBeInvalid_WhenReleaseWindowIdIsEmptyAndReleaseJobIsNull()
        {
            var command = new UpdateReleaseJobCommand
            {
                ReleaseWindowId = Guid.Empty,
                ReleaseJob = null
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(2, result.Errors.Count, "error list size");
            Assert.AreEqual(result.Errors[0].PropertyName, "ReleaseWindowId");
            Assert.AreEqual(result.Errors[1].PropertyName, "ReleaseJob");
        }

        [Test]
        public void Validate_ShouldBeInvalid_WhenReleaseWindowIdAndReleaseJobAreEmpty()
        {
            var command = new UpdateReleaseJobCommand
            {
                ReleaseWindowId = Guid.Empty,
                ReleaseJob = new ReleaseJob()
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(4, result.Errors.Count, "error list size");
            Assert.AreEqual(result.Errors[0].PropertyName, "ReleaseWindowId");
            Assert.AreEqual(result.Errors[1].PropertyName, "ReleaseJob.JobId");
            Assert.AreEqual(result.Errors[2].PropertyName, "ReleaseJob.Name");
            Assert.AreEqual(result.Errors[3].PropertyName, "ReleaseJob.ExternalId");
        }


        [Test]
        public void Validate_ShouldBeValid_WhenCommandIsCorrect()
        {
            var command = new UpdateReleaseJobCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                ReleaseJob = new ReleaseJob
                {
                    ExternalId = Guid.NewGuid(),
                    Name = RandomData.RandomString(10),
                    JobId = Guid.NewGuid()
                }
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
