using System;
using Moq;
using NUnit.Framework;
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
    public class DeleteReleaseTaskCommandHandlerTests : TestClassFor<DeleteReleaseTaskCommandHandler>
    {
        private Mock<IReleaseTaskGateway> _releaseTaskGatewayFactoryMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;

        protected override DeleteReleaseTaskCommandHandler ConstructSystemUnderTest()
        {
            return new DeleteReleaseTaskCommandHandler
            {
                PublishEvent = _eventPublisherMock.Object,
                ReleaseTaskGatewayFactory = () => _releaseTaskGatewayFactoryMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _eventPublisherMock = new Mock<IPublishEvent>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            _releaseTaskGatewayFactoryMock = new Mock<IReleaseTaskGateway>();

            base.TestInitialize();
        }

        [Test]
        public void DeleteReleaseTask_ShouldCallGatewayMethodAndDisposeIt_WhenCallCallGateway()
        {
            var command = new DeleteReleaseTaskCommand
            {
                CommandContext = new CommandContext(),
                ReleaseTaskId = Guid.NewGuid()
            };
            var releaseTask = new ReleaseTask
            {
                ExternalId = command.ReleaseTaskId,
                HelpDeskTicketReference = RandomData.RandomString(10),
                ReleaseWindowId = Guid.NewGuid()
            };
            _releaseTaskGatewayFactoryMock.Setup(x => x.GetReleaseTask(command.ReleaseTaskId))
                .Returns(releaseTask);

            Sut.Handle(command);

            _releaseTaskGatewayFactoryMock.Verify(x => x.GetReleaseTask(command.ReleaseTaskId), Times.Once);
            _releaseTaskGatewayFactoryMock.Verify(x => x.DeleteReleaseTask(command.ReleaseTaskId), Times.Once);
            _releaseTaskGatewayFactoryMock.Verify(x => x.Dispose(), Times.Once);
            _commandDispatcherMock.Verify(x => x.Send(It.Is<DeleteHelpDeskTaskCommand>(c => c.HelpDeskTicketRef == releaseTask.HelpDeskTicketReference)), Times.Once);
            _eventPublisherMock.Verify(x => x.Publish(It.Is<ReleaseTaskDeletedEvent>(e =>
                e.ReleaseTask == releaseTask
                && e.ReleaseWindowId == releaseTask.ReleaseWindowId)), Times.Once);
        }

        [Test]
        public void DeleteReleaseTask_ShouldNotDeleteHelpDeskTicket_WhenThereIsNoReference()
        {
            var command = new DeleteReleaseTaskCommand
            {
                CommandContext = new CommandContext(),
                ReleaseTaskId = Guid.NewGuid()
            };
            var releaseTask = new ReleaseTask
            {
                ExternalId = command.ReleaseTaskId,
                HelpDeskTicketReference = null
            };
            _releaseTaskGatewayFactoryMock.Setup(x => x.GetReleaseTask(command.ReleaseTaskId))
                .Returns(releaseTask);

            Sut.Handle(command);

            _releaseTaskGatewayFactoryMock.Verify(x => x.GetReleaseTask(command.ReleaseTaskId), Times.Once);
            _releaseTaskGatewayFactoryMock.Verify(x => x.DeleteReleaseTask(command.ReleaseTaskId), Times.Once);
            _releaseTaskGatewayFactoryMock.Verify(x => x.Dispose(), Times.Once);
            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<DeleteHelpDeskTaskCommand>()), Times.Never);
            _eventPublisherMock.Verify(x => x.Publish(It.Is<ReleaseTaskDeletedEvent>(e => e.ReleaseTask == releaseTask)), Times.Once);
        }
    }
}
