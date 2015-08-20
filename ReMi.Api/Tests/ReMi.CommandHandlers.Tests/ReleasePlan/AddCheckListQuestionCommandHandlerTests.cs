using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class AddCheckListQuestionCommandHandlerTests : TestClassFor<AddCheckListQuestionCommandHandler>
    {
        private Mock<ICheckListGateway> _checkListGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override AddCheckListQuestionCommandHandler ConstructSystemUnderTest()
        {
            return new AddCheckListQuestionCommandHandler
            {
                CheckListGatewayFactory = () => _checkListGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _checkListGatewayMock = new Mock<ICheckListGateway>();
            _eventPublisherMock = new Mock<IPublishEvent>();

            base.TestInitialize();
        }

        [Test]
        public void AddChecks_ShouldAddNewAndAssociateExisting_WhenCalled()
        {
            var checkListQuestionToAdd = new CheckListQuestion
            {
                ExternalId = Guid.NewGuid(),
                Question = RandomData.RandomString(13)
            };
            var checkListQuestionToAssign = new CheckListQuestion
            {
                ExternalId = Guid.NewGuid(),
                Question = RandomData.RandomString(13)
            };
            var command = new AddCheckListQuestionsCommand
            {
                QuestionsToAdd = new[] {checkListQuestionToAdd},
                QuestionsToAssign = new[] {checkListQuestionToAssign},
                ReleaseWindowId = Guid.NewGuid()
            };

            Sut.Handle(command);

            _checkListGatewayMock.Verify(c => c.AddCheckListQuestions(command.QuestionsToAdd, command.ReleaseWindowId));
            _checkListGatewayMock.Verify(c => c.AssociateCheckListQuestionWithPackage(command.QuestionsToAssign, command.ReleaseWindowId));
            _eventPublisherMock.Verify(e => e.Publish(
                It.Is<CheckListQuestionsAddedEvent>(
                    c => c.ReleaseWindowGuid == command.ReleaseWindowId
                        && c.Questions.Count() == 2
                        && c.Questions.Contains(checkListQuestionToAdd)
                        && c.Questions.Contains(checkListQuestionToAssign))));
        }
    }
}
