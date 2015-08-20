using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.CommandValidators.ReleasePlan;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleasePlan
{
    public class UpdateCheckListCommandValidatorTests : TestClassFor<UpdateCheckListCommandValidator>
    {
        protected override UpdateCheckListCommandValidator ConstructSystemUnderTest()
        {
            return new UpdateCheckListCommandValidator();
        }

        [Test]
        public void Validate_ShouldNotPass_WhenCommentLengtIsInvalid()
        {
            var command = new UpdateCheckListCommentCommand
            {
                CheckListItem = new CheckListItem
                {
                    Comment = new string('*', 4001)
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void Validate_ShouldPass_WhenCommentLengtIsValid()
        {
            var command = new UpdateCheckListCommentCommand
            {
                CheckListItem = new CheckListItem
                {
                    Comment = "hello world"
                }
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }
    }
}
