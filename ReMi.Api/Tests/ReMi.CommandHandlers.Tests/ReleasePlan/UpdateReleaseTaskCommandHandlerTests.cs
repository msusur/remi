using System;
using FizzWare.NBuilder;
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
    public class UpdateReleaseTaskCommandHandlerTests : TestClassFor<UpdateReleaseTaskCommandHandler>
    {
        private Mock<IReleaseTaskGateway> _releaseTaskGatewayFactoryMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;

        protected override UpdateReleaseTaskCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateReleaseTaskCommandHandler
            {
                CommandDispatcher = _commandDispatcherMock.Object,
                PublishEvent = _eventPublisherMock.Object,
                ReleaseTaskGatewayFactory = () => _releaseTaskGatewayFactoryMock.Object
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
        public void Handle_ShouldDisposeGateways_WhenInvoked()
        {
            var releaseTask = new ReleaseTask { HelpDeskTicketReference = RandomData.RandomString(10), ExternalId = Guid.NewGuid() };

            _releaseTaskGatewayFactoryMock.Setup(x => x.GetReleaseTask(releaseTask.ExternalId))
                .Returns(releaseTask);
            _releaseTaskGatewayFactoryMock.Setup(o => o.UpdateReleaseTask(It.IsAny<ReleaseTask>())).Returns(true);

            Sut.Handle(new UpdateReleaseTaskCommand { ReleaseTask = releaseTask });

            _releaseTaskGatewayFactoryMock.Verify(x => x.Dispose());
        }

        [Test]
        public void Handle_ShouldCallGatewayMethod_WhenInvoked()
        {
            var releaseTask = BuildReleaseTask();

            _releaseTaskGatewayFactoryMock.Setup(x => x.GetReleaseTask(releaseTask.ExternalId))
                .Returns(releaseTask);
            _releaseTaskGatewayFactoryMock.Setup(o => o.UpdateReleaseTask(It.IsAny<ReleaseTask>())).Returns(true);

            Sut.Handle(new UpdateReleaseTaskCommand { ReleaseTask = releaseTask });

            _releaseTaskGatewayFactoryMock.Verify(x => x.UpdateReleaseTask(releaseTask));
        }

        [Test]
        public void Handle_ShouldSendCleanReleaseTaskReceiptCommand()
        {
            var releaseTask = BuildReleaseTask();

            _releaseTaskGatewayFactoryMock.Setup(x => x.GetReleaseTask(releaseTask.ExternalId))
                .Returns(releaseTask);
            _releaseTaskGatewayFactoryMock.Setup(o => o.UpdateReleaseTask(It.IsAny<ReleaseTask>())).Returns(true);

            Sut.Handle(new UpdateReleaseTaskCommand { ReleaseTask = releaseTask });

            _commandDispatcherMock.Verify(
                cd => cd.Send(It.Is<CleanReleaseTaskReceiptCommand>(c => c.ReleaseTaskId == releaseTask.ExternalId)));
        }

        [Test]
        public void Handle_ShouldPublishUpdateHelpDeskTaskCommand_WhenHelpDeskRefPresent()
        {
            var releaseTask = BuildReleaseTask(true);

            _releaseTaskGatewayFactoryMock.Setup(x => x.GetReleaseTask(releaseTask.ExternalId))
                .Returns(releaseTask);
            _releaseTaskGatewayFactoryMock.Setup(o => o.UpdateReleaseTask(It.IsAny<ReleaseTask>())).Returns(true);

            Sut.Handle(new UpdateReleaseTaskCommand { ReleaseTask = releaseTask });

            _commandDispatcherMock.Verify(x => x.Send(It.Is<UpdateHelpDeskTaskCommand>(e => e.ReleaseTask == releaseTask)));
        }

        [Test]
        public void Handle_ShouldPublishReleaseTaskUpdatedEvent_WhenNotHelpDeskReleaseTaskWasUpdated()
        {
            var releaseTask = BuildReleaseTask();

            _releaseTaskGatewayFactoryMock.Setup(x => x.GetReleaseTask(releaseTask.ExternalId))
                .Returns(releaseTask);
            _releaseTaskGatewayFactoryMock.Setup(o => o.UpdateReleaseTask(It.IsAny<ReleaseTask>())).Returns(true);

            Sut.Handle(new UpdateReleaseTaskCommand { ReleaseTask = releaseTask });

            _eventPublisherMock.Verify(x => x.Publish(It.Is<ReleaseTaskUpdatedEvent>(e => e.ReleaseTask == releaseTask)));
        }

        [Test]
        public void Handle_ShouldNotPublishEvents_WhenNotChangesSavedInRepository()
        {
            var releaseTask = new ReleaseTask { ExternalId = Guid.NewGuid() };

            _releaseTaskGatewayFactoryMock.Setup(x => x.GetReleaseTask(releaseTask.ExternalId))
                .Returns(releaseTask);
            _releaseTaskGatewayFactoryMock.Setup(o => o.UpdateReleaseTask(It.IsAny<ReleaseTask>())).Returns(false);

            Sut.Handle(new UpdateReleaseTaskCommand { ReleaseTask = releaseTask });

            _eventPublisherMock.Verify(x => x.Publish(It.Is<ReleaseTaskUpdatedEvent>(e => e.ReleaseTask == releaseTask)), Times.Never);
            _commandDispatcherMock.Verify(x => x.Send(It.Is<CleanReleaseTaskReceiptCommand>(e => e.ReleaseTaskId == releaseTask.ExternalId)), Times.Never);
        }

        [Test]
        public void Handle_ShouldSendCreateHelpDeskTicketCommand_WhenOnlyCreateHelpDeskTicketPropertyChanged()
        {
            var releaseTask = new ReleaseTask { ExternalId = Guid.NewGuid(), CreateHelpDeskTicket = true };

            _releaseTaskGatewayFactoryMock.Setup(x => x.GetReleaseTask(releaseTask.ExternalId))
                .Returns(releaseTask);
            _releaseTaskGatewayFactoryMock.Setup(o => o.UpdateReleaseTask(It.IsAny<ReleaseTask>())).Returns(false);

            Sut.Handle(new UpdateReleaseTaskCommand { ReleaseTask = releaseTask });

            _commandDispatcherMock.Verify(x => x.Send(It.Is<CreateHelpDeskTaskCommand>(e => e.ReleaseTask.ExternalId == releaseTask.ExternalId)), Times.Once);
        }

        private ReleaseTask BuildReleaseTask(bool helpDesk = false)
        {
            var releaseTask = Builder<ReleaseTask>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.HelpDeskTicketReference, helpDesk ? RandomData.RandomString(10) : null)
                .With(o => o.HelpDeskTicketUrl, helpDesk ? RandomData.RandomString(200) : null)
                .Build();

            return releaseTask;
        }
    }
}
