using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;
using System;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class RemoveCheckListQuestionCommandHandlerTests : TestClassFor<RemoveCheckListQuestionCommandHandler>
    {
        private Mock<ICheckListGateway> _checkListGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override RemoveCheckListQuestionCommandHandler ConstructSystemUnderTest()
        {
            return new RemoveCheckListQuestionCommandHandler
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
        public void RemoveCheck_ShouldRemoveCheckForCurrentRelease()
        {
            var checkListItem = new CheckListItemView
            {
                ExternalId = Guid.NewGuid(),
                CheckListQuestion = RandomData.RandomString(13),
                ReleaseWindowId = Guid.NewGuid()
            };
            _checkListGatewayMock.Setup(x => x.GetCheckListItem(checkListItem.ExternalId))
                .Returns(new CheckListItemView { ReleaseWindowId = checkListItem.ReleaseWindowId });

            Sut.Handle(new RemoveCheckListQuestionCommand { CheckListQuestionId = checkListItem.ExternalId, ForWholeProduct = false });

            _checkListGatewayMock.Verify(x => x.GetCheckListItem(It.IsAny<Guid>()), Times.Once);
            _checkListGatewayMock.Verify(c => c.RemoveCheckListQuestion(checkListItem.ExternalId));
            _eventPublisherMock.Verify(e => e.Publish(It.Is<CheckListQuestionRemovedEvent>(c =>
                c.ReleaseWindowGuid == checkListItem.ReleaseWindowId
                && c.CheckListId == checkListItem.ExternalId)));
        }

        [Test]
        public void RemoveCheck_ShouldRemoveCheckForWholeProduct()
        {
            var checkListItem = new CheckListItemView
            {
                ExternalId = Guid.NewGuid(),
                CheckListQuestion = RandomData.RandomString(13),
                ReleaseWindowId = Guid.NewGuid()
            };
            _checkListGatewayMock.Setup(x => x.GetCheckListItem(checkListItem.ExternalId))
                .Returns(new CheckListItemView { ReleaseWindowId = checkListItem.ReleaseWindowId });

            Sut.Handle(new RemoveCheckListQuestionCommand { CheckListQuestionId = checkListItem.ExternalId, ForWholeProduct = true });

            _checkListGatewayMock.Verify(x => x.GetCheckListItem(It.IsAny<Guid>()), Times.Once);
            _checkListGatewayMock.Verify(c => c.RemoveCheckListQuestionForPackage(checkListItem.ExternalId));
            _eventPublisherMock.Verify(e => e.Publish(It.Is<CheckListQuestionRemovedEvent>(c =>
                c.ReleaseWindowGuid == checkListItem.ReleaseWindowId &&
                c.CheckListId == checkListItem.ExternalId)));
        }
    }
}
