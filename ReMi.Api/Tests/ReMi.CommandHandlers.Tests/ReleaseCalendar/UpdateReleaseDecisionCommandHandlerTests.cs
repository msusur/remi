using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Constants;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Enums;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class UpdateReleaseDecisionCommandHandlerTests : TestClassFor<UpdateReleaseDecisionCommandHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IPublishEvent> _publishEventMock;

        protected override UpdateReleaseDecisionCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateReleaseDecisionCommandHandler
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                PublishEvent = _publishEventMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _publishEventMock = new Mock<IPublishEvent>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldUpdateReleaseWindowDecision_WhenInvoked()
        {
            var command = new UpdateReleaseDecisionCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                ReleaseDecision = RandomData.RandomEnum<ReleaseDecision>()
            };

            Sut.Handle(command);

            _releaseWindowGatewayMock.Verify(a => a.UpdateReleaseDecision(command.ReleaseWindowId, command.ReleaseDecision), Times.Once);
            _publishEventMock.Verify(a => a.Publish(It.Is<ReleaseDecisionChangedEvent>(x =>
                    x.ReleaseWindowId == command.ReleaseWindowId
                    && x.ReleaseDecision == EnumDescriptionHelper.GetDescription(command.ReleaseDecision))),
                Times.Once);
        }
    }
}
