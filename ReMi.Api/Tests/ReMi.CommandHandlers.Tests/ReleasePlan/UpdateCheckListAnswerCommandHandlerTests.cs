using System;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class UpdateCheckListAnswerCommandHandlerTests : TestClassFor<UpdateCheckListAnswerCommandHandler>
    {
        private Mock<ICheckListGateway> _checkListGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override UpdateCheckListAnswerCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateCheckListAnswerCommandHandler
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
        public void UpdateChecklistAnswer_ShouldCallGateway()
        {
            var checkListItem = new CheckListItem
            {
                ExternalId = Guid.NewGuid(),
                LastChangedBy = RandomData.RandomString(10, 25),
                Checked = RandomData.RandomBool()
            };
            var checkListItemView = new CheckListItemView {ReleaseWindowId = Guid.NewGuid()};
            _checkListGatewayMock.Setup(c => c.GetCheckListItem(checkListItem.ExternalId)).Returns(checkListItemView);
            var command = new UpdateCheckListAnswerCommand
            {
                CheckListItem = checkListItem,
                CommandContext = new CommandContext { UserName = RandomData.RandomString(50) }
            };
            
            Sut.Handle(command);

            _checkListGatewayMock.Verify(c => c.UpdateAnswer(checkListItem));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<CheckListAnswerUpdatedEvent>(
                            c =>
                                c.AnsweredBy == checkListItem.LastChangedBy &&
                                c.ReleaseWindowId == checkListItemView.ReleaseWindowId &&
                                c.Checked == checkListItem.Checked && 
                                c.CheckListId == checkListItem.ExternalId)));
        }
    }
}
