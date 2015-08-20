using System;
using System.Linq;
using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.Commands.SourceControl;
using ReMi.CommandValidators.SourceControl;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data.SourceControl;

namespace ReMi.CommandValidators.Tests.SourceControl
{
    public class UpdateReleaseRepositoryCommandValidatorTests : TestClassFor<UpdateReleaseRepositoryCommandValidator>
    {
        protected override UpdateReleaseRepositoryCommandValidator ConstructSystemUnderTest()
        {
            return new UpdateReleaseRepositoryCommandValidator();
        }

        [Test]
        public void Validate_ShouldNotPass_WhenCommandIsEmpty()
        {
            var command = new UpdateReleaseRepositoryCommand();

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("ReleaseWindowId", result.Errors.ElementAt(0).PropertyName);
            Assert.AreEqual("Repository", result.Errors.ElementAt(1).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenReleaseWindowIdIsEmptyAndRepositoryIsEmpty()
        {
            var command = new UpdateReleaseRepositoryCommand
            {
                ReleaseWindowId = Guid.Empty,
                Repository = new ReleaseRepository()
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(5, result.Errors.Count);
            Assert.AreEqual("ReleaseWindowId", result.Errors.ElementAt(0).PropertyName);
            Assert.AreEqual("Repository.ChangesFrom", result.Errors.ElementAt(1).PropertyName);
            Assert.AreEqual("Repository.ChangesTo", result.Errors.ElementAt(2).PropertyName);
            Assert.AreEqual("Repository.ExternalId", result.Errors.ElementAt(3).PropertyName);
            Assert.AreEqual("Repository.Repository", result.Errors.ElementAt(4).PropertyName);
        }

        [Test]
        public void Validate_ShouldPass_WhenCommandIsValid()
        {
            var command = new UpdateReleaseRepositoryCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                Repository = Builder<ReleaseRepository>.CreateNew().Build() 
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
