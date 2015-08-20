using System;
using System.Linq;
using NUnit.Framework;
using ReMi.Commands.SourceControl;
using ReMi.CommandValidators.SourceControl;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.SourceControl
{
    public class LoadReleaseRepositoriesCommandValidatorTests : TestClassFor<LoadReleaseRepositoriesCommandValidator>
    {
        protected override LoadReleaseRepositoriesCommandValidator ConstructSystemUnderTest()
        {
            return new LoadReleaseRepositoriesCommandValidator();
        }

        [Test]
        public void Validate_ShouldNotPass_WhenCommandIsEmpty()
        {
            var command = new LoadReleaseRepositoriesCommand();

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("ReleaseWindowId", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldPass_WhenCommandIsValid()
        {
            var command = new LoadReleaseRepositoriesCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
