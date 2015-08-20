using System;
using System.Linq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.CommandValidators.ReleasePlan;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleasePlan
{
    public class RemoveCheckListQuestionCommandValidatorTests : TestClassFor<RemoveCheckListQuestionCommandValidator>
    {
        protected override RemoveCheckListQuestionCommandValidator ConstructSystemUnderTest()
        {
            return new RemoveCheckListQuestionCommandValidator();
        }

        [Test]
        public void Validate_ShouldNotPass_WhenCommandIsEmpty()
        {
            var command = new RemoveCheckListQuestionCommand();

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("CheckListQuestionId", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenCheckListQuestionIdIsEmpty()
        {
            var command = new RemoveCheckListQuestionCommand
            {
                CheckListQuestionId = Guid.Empty,
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("CheckListQuestionId", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenCommandIsValid()
        {
            var command = new RemoveCheckListQuestionCommand
            {
                CheckListQuestionId = Guid.NewGuid(),
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
