using System;
using NUnit.Framework;
using ReMi.Commands.Configuration;
using ReMi.Commands.DeploymentTool;
using ReMi.CommandValidators.DeploymentTool;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.DeploymentTool
{
    public class PopulateDeploymentMeasurementsCommandValidatorTests : TestClassFor<PopulateDeploymentMeasurementsCommandValidator>
    {
        protected override PopulateDeploymentMeasurementsCommandValidator ConstructSystemUnderTest()
        {
            return new PopulateDeploymentMeasurementsCommandValidator();
        }

        [Test]
        public void Validate_ShouldBeInvalid_WhenReleaseWindowIdIsEmpty()
        {
            var command = new PopulateDeploymentMeasurementsCommand
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
            var command = new PopulateDeploymentMeasurementsCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
