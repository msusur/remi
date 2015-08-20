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
    public class ReleaseTaskHandlerTests : TestClassFor<CompleteReleaseTaskCommandHandler>
    {
        private Mock<IReleaseTaskGateway> _releaseTaskGatewayMock;
        private Mock<IPublishEvent> _eventPublisher;

        protected override CompleteReleaseTaskCommandHandler ConstructSystemUnderTest()
        {
            return new CompleteReleaseTaskCommandHandler
            {
                EventPublisher = _eventPublisher.Object,
                ReleaseTaskGatewayFactory = () => _releaseTaskGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseTaskGatewayMock = new Mock<IReleaseTaskGateway>();
            _eventPublisher = new Mock<IPublishEvent>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallReleaseTaskGatewayAndSendEvent()
        {
            var releaseWindowId = Guid.NewGuid();
            var command = new CompleteReleaseTaskCommand
            {
                CommandContext = new CommandContext { UserId = Guid.NewGuid() },
                ReleaseTaskExtetnalId = Guid.NewGuid()
            };
            var assignee = RandomData.RandomString(43);
            _releaseTaskGatewayMock.Setup(r => r.GetReleaseTask(command.ReleaseTaskExtetnalId))
                .Returns(new ReleaseTask
                {
                    Assignee = assignee,
                    ReleaseWindowId = releaseWindowId
                });

            Sut.Handle(command);

            _releaseTaskGatewayMock.Verify(
                r => r.CompleteTask(command.ReleaseTaskExtetnalId, command.CommandContext.UserId));
            _releaseTaskGatewayMock.Verify(r=>r.GetReleaseTask(command.ReleaseTaskExtetnalId));
            _eventPublisher.Verify(
                e =>
                    e.Publish(
                        It.Is<TaskCompletedEvent>(
                            t =>
                                t.ReleaseTaskExternalId == command.ReleaseTaskExtetnalId &&
                                t.ReleaseWindowExternalId == releaseWindowId && t.AssigneeName == assignee)));
        }
    }
}
