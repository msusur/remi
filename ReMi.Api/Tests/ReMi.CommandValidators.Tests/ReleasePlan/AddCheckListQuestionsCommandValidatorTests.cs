using System;
using System.Linq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.CommandValidators.ReleasePlan;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleasePlan
{
    public class AddCheckListQuestionsCommandValidatorTests : TestClassFor<AddCheckListQuestionsCommandValidator>
    {
        protected override AddCheckListQuestionsCommandValidator ConstructSystemUnderTest()
        {
            return new AddCheckListQuestionsCommandValidator();
        }

        [Test]
        public void Validate_ShouldNotPass_WhenCommandIsEmpty()
        {
            var command = new AddCheckListQuestionsCommand();

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(3, result.Errors.Count);
            Assert.AreEqual("ReleaseWindowId", result.Errors.ElementAt(0).PropertyName);
            Assert.AreEqual("QuestionsToAdd", result.Errors.ElementAt(1).PropertyName);
            Assert.AreEqual("QuestionsToAssign", result.Errors.ElementAt(2).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenQuestionToAddOrQuestionToAssignIsInvalid()
        {
            var command = new AddCheckListQuestionsCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                QuestionsToAdd = new[] { new CheckListQuestion() },
                QuestionsToAssign = new[] { new CheckListQuestion() }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(6, result.Errors.Count);
            Assert.AreEqual("QuestionsToAdd[0].Question", result.Errors.ElementAt(0).PropertyName);
            Assert.AreEqual("QuestionsToAdd[0].ExternalId", result.Errors.ElementAt(1).PropertyName);
            Assert.AreEqual("QuestionsToAdd[0].CheckListId", result.Errors.ElementAt(2).PropertyName);
            Assert.AreEqual("QuestionsToAssign[0].Question", result.Errors.ElementAt(3).PropertyName);
            Assert.AreEqual("QuestionsToAssign[0].ExternalId", result.Errors.ElementAt(4).PropertyName);
            Assert.AreEqual("QuestionsToAssign[0].CheckListId", result.Errors.ElementAt(5).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenReleaseWindowIdIsEmpty()
        {
            var command = new AddCheckListQuestionsCommand
            {
                ReleaseWindowId = Guid.Empty,
                QuestionsToAdd = new[] { new CheckListQuestion
                {
                    ExternalId = Guid.NewGuid(),
                    Question = RandomData.RandomString(10),
                    CheckListId = Guid.NewGuid()
                } }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("ReleaseWindowId", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldPass_WhenCommandIsValid()
        {
            var command = new AddCheckListQuestionsCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                QuestionsToAdd = new[] { new CheckListQuestion
                {
                    ExternalId = Guid.NewGuid(),
                    Question = RandomData.RandomString(10),
                    CheckListId = Guid.NewGuid()
                } },
                QuestionsToAssign = new[] { new CheckListQuestion
                {
                    ExternalId = Guid.NewGuid(),
                    Question = RandomData.RandomString(10),
                    CheckListId = Guid.NewGuid()
                } }
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
