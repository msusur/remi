using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ContinuousDelivery;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants.ContinuousDelivery;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Enums;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleasePlan;
using ReMi.Queries.ContinuousDelivery;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class CheckQaStatusCommandHandlerTests : TestClassFor<CheckQaStatusCommandHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;

        private Mock<IHandleQuery<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse>>
            _getStautsQueryMock;

        private ReleaseWindow _releaseWindow;
        private GetContinuousDeliveryStatusResponse _statusResponse;
        private CheckQaStatusCommand _command;
        private string _productName;

        protected override CheckQaStatusCommandHandler ConstructSystemUnderTest()
        {
            return new CheckQaStatusCommandHandler
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object,
                EventPublisher = _eventPublisherMock.Object,
                StatusQuery = _getStautsQueryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productName = RandomData.RandomString(5);

            _statusResponse = new GetContinuousDeliveryStatusResponse
            {
                Products = new[] { _productName },
                StatusCheck = new List<StatusCheckItem>
                    {
                        new StatusCheckItem
                        {
                            MetricControl = "test",
                            Status = StatusType.Red
                        }
                    }
            };

            _releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                Products = new[] { _productName },
                ReleaseDecision = EnumDescriptionHelper.GetDescription(ReleaseDecision.NoGo),
                ApprovedOn = DateTime.UtcNow.AddDays(-1)
            };

            _command = new CheckQaStatusCommand
            {
                ReleaseWindowId = _releaseWindow.ExternalId
            };

            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _getStautsQueryMock =
                new Mock<IHandleQuery<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse>>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            _eventPublisherMock = new Mock<IPublishEvent>();

            _getStautsQueryMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()))
                .Returns(_statusResponse);

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(_releaseWindow.ExternalId, false, It.IsAny<Boolean>()))
                .Returns(_releaseWindow);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldSendStatusCheckedEvent_WhenDecisionWasNotChanged()
        {
            Sut.Handle(_command);

            _releaseWindowGatewayMock.Verify(
                x => x.GetByExternalId(_releaseWindow.ExternalId, false, It.IsAny<Boolean>()));

            _getStautsQueryMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()));

            _eventPublisherMock.Verify(
                x =>
                    x.Publish(
                        It.Is<QaStatusCheckedEvent>(
                            e =>
                                e.ReleaseWindowId == _command.ReleaseWindowId &&
                                e.StatusCheckItems == _statusResponse.StatusCheck)));

            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<UpdateReleaseDecisionCommand>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldSendStatusCheckedEvent_WhenReleaseWasNotApproved()
        {
            _statusResponse.StatusCheck.ToArray()[0].Status = StatusType.Green;
            _releaseWindow.ApprovedOn = null;

            Sut.Handle(_command);

            _releaseWindowGatewayMock.Verify(
                x => x.GetByExternalId(_releaseWindow.ExternalId, false, It.IsAny<Boolean>()));

            _getStautsQueryMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()));

            _eventPublisherMock.Verify(
                x =>
                    x.Publish(
                        It.Is<QaStatusCheckedEvent>(
                            e =>
                                e.ReleaseWindowId == _command.ReleaseWindowId &&
                                e.StatusCheckItems == _statusResponse.StatusCheck)));

            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<UpdateReleaseDecisionCommand>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldSendStatusCheckedEventAndUpdatedecisionCommand_WhenDecisionIsGo()
        {
            _statusResponse.StatusCheck.ToArray()[0].Status = StatusType.Green;

            Sut.Handle(_command);

            _releaseWindowGatewayMock.Verify(
                x => x.GetByExternalId(_releaseWindow.ExternalId, false, It.IsAny<Boolean>()));

            _getStautsQueryMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()));

            _eventPublisherMock.Verify(
                x =>
                    x.Publish(
                        It.Is<QaStatusCheckedEvent>(
                            e =>
                                e.ReleaseWindowId == _command.ReleaseWindowId &&
                                e.StatusCheckItems == _statusResponse.StatusCheck)));

            _commandDispatcherMock.Verify(
                x =>
                    x.Send(
                        It.Is<UpdateReleaseDecisionCommand>(
                            c =>
                                c.ReleaseDecision == ReleaseDecision.Go && c.ReleaseWindowId == _command.ReleaseWindowId)));
        }

        [Test]
        public void Handle_ShouldSendStatusCheckedEventAndUpdatedecisionCommand_WhenDecisionIsNoGo()
        {
            _releaseWindow.ReleaseDecision = EnumDescriptionHelper.GetDescription(StatusType.Green);

            Sut.Handle(_command);

            _releaseWindowGatewayMock.Verify(
                x => x.GetByExternalId(_releaseWindow.ExternalId, false, It.IsAny<Boolean>()));

            _getStautsQueryMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()));

            _eventPublisherMock.Verify(
                x =>
                    x.Publish(
                        It.Is<QaStatusCheckedEvent>(
                            e =>
                                e.ReleaseWindowId == _command.ReleaseWindowId &&
                                e.StatusCheckItems == _statusResponse.StatusCheck)));

            _commandDispatcherMock.Verify(
                x =>
                    x.Send(
                        It.Is<UpdateReleaseDecisionCommand>(
                            c =>
                                c.ReleaseDecision == ReleaseDecision.NoGo && c.ReleaseWindowId == _command.ReleaseWindowId)));
        }
    }
}
