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
    public class UpdateReleaseTasksOrderCommandHandlerTests : TestClassFor<UpdateReleaseTasksOrderCommandHandler>
    {
        private Mock<IReleaseTaskGateway> _releaseTaskGatewayMock;
        private Mock<IPublishEvent> _publishEventMock;

        protected override UpdateReleaseTasksOrderCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateReleaseTasksOrderCommandHandler
            {
                ReleaseTaskGatewayFactory = () => _releaseTaskGatewayMock.Object,
                PublishEvent = _publishEventMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseTaskGatewayMock = new Mock<IReleaseTaskGateway>();
            _publishEventMock = new Mock<IPublishEvent>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenInvoked()
        {
            var command = new UpdateReleaseTasksOrderCommand
            {
                ReleaseTasksOrder = new Dictionary<Guid, short>
                {
                    { Guid.NewGuid(), 1 }
                }
            };
            var releaseTask = new ReleaseTask
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            _releaseTaskGatewayMock.Setup(x => x.GetReleaseTask(command.ReleaseTasksOrder.First().Key))
                .Returns(releaseTask);

            Sut.Handle(command);

            _releaseTaskGatewayMock.Verify(x => x.UpdateReleaseTasksOrder(command.ReleaseTasksOrder), Times.Once());
            _releaseTaskGatewayMock.Verify(x => x.GetReleaseTask(command.ReleaseTasksOrder.First().Key), Times.Once());
            _publishEventMock.Verify(x => x.Publish(It.Is<ReleaseTasksOrderUpdatedEvent>(evt => evt.ReleaseWindowId == releaseTask.ReleaseWindowId)), Times.Once());
        }
    }
}
