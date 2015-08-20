using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.CommandHandlers.Acknowledge;
using ReMi.Commands.Acknowledge;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;
using System;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class AcknowledgeHandlerTests : TestClassFor<AcknowledgeHandler>
    {
        private readonly Mock<IReleaseParticipantGateway> _releaseParticipantGatewayMock =
            new Mock<IReleaseParticipantGateway>();

        private Mock<IPublishEvent> _eventPublisherMock;

        private ApproveReleaseParticipationCommand _approveAcknowledgementCommand;
        private ClearReleaseAcknowledgesCommand _clearReleaseAcknowledgesCommand;
        private readonly Guid _authorId = Guid.NewGuid();
        private ReleaseWindow _releaseWindow;
        
        protected override AcknowledgeHandler ConstructSystemUnderTest()
        {
            return new AcknowledgeHandler
            {
                ReleaseParticipantGatewayFactory = () => _releaseParticipantGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid()
            };
            _eventPublisherMock = new Mock<IPublishEvent>();
            _approveAcknowledgementCommand = new ApproveReleaseParticipationCommand
            {
                ReleaseParticipantId = Guid.NewGuid()
            };
            _clearReleaseAcknowledgesCommand = new ClearReleaseAcknowledgesCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext { UserId = _authorId } 
            };
            _releaseParticipantGatewayMock.Setup(
                x => x.ApproveReleaseParticipation(_approveAcknowledgementCommand.ReleaseParticipantId));
            _releaseParticipantGatewayMock.Setup(
                x =>
                    x.ClearParticipationApprovements(_clearReleaseAcknowledgesCommand.ReleaseWindowId,
                        _authorId));
            _releaseParticipantGatewayMock.Setup(
                r => r.GetReleaseWindow(_approveAcknowledgementCommand.ReleaseParticipantId)).Returns(_releaseWindow);

            base.TestInitialize();
        }

        [Test]
        public void AcknowledgeParticipation_ShouldCallGatewayCorrectly()
        {
            Sut.Handle(_approveAcknowledgementCommand);

            _releaseParticipantGatewayMock.Verify(
                x => x.ApproveReleaseParticipation(_approveAcknowledgementCommand.ReleaseParticipantId));
            _releaseParticipantGatewayMock.Verify(
                r => r.GetReleaseWindow(_approveAcknowledgementCommand.ReleaseParticipantId));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseParticipationConfirmedEvent>(
                            r =>
                                r.ReleaseParticipantId == _approveAcknowledgementCommand.ReleaseParticipantId &&
                                r.ReleaseWindowGuid == _releaseWindow.ExternalId)));
        }

        [Test]
        public void ClearAcknowledges_WhenReleaseWindowUpdates_ShouldCallGatewayCorrectly()
        {
            Sut.Handle(_clearReleaseAcknowledgesCommand);

            _releaseParticipantGatewayMock.Setup(
                x => x.ClearParticipationApprovements(_clearReleaseAcknowledgesCommand.ReleaseWindowId, _authorId));
        }
    }
}
