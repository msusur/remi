using NUnit.Framework;
using ReMi.Commands.DeploymentTool;
using ReMi.CommandValidators.Metrics;
using ReMi.TestUtils.UnitTests;
using System;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.Commands.Metrics;

namespace ReMi.CommandValidators.Tests.Metrics
{
    public class CalculateDeployTimeCommandValidatorTests : TestClassFor<CalculateDeployTimeCommandValidator>
    {
        protected override CalculateDeployTimeCommandValidator ConstructSystemUnderTest()
        {
            return new CalculateDeployTimeCommandValidator();
        }

        [Test]
        public void Validate_ShouldBeInvalid_WhenCommandIsEmpty()
        {
            var command = new CalculateDeployTimeCommand
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
            var command = new CalculateDeployTimeCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
