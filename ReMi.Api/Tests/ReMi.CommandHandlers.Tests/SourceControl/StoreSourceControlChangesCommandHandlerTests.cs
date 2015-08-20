using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.SourceControl;
using ReMi.Commands.SourceControl;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Data.Exceptions;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;
using ReMi.Events;
using ReMi.Queries.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.SourceControl
{
    public class StoreSourceControlChangesCommandHandlerTests : TestClassFor<StoreSourceControlChangesCommandHandler>
    {
        private Mock<ISourceControlChangeGateway> _sourceControlGatewayMock;
        private Mock<IHandleQuery<GetReleaseChangesRequest, GetReleaseChangesResponse>> _changesQueryMock;
        private Mock<IPublishEvent> _publishEventMock;

        protected override StoreSourceControlChangesCommandHandler ConstructSystemUnderTest()
        {
            return new StoreSourceControlChangesCommandHandler
            {
                SourceControlChangeGatewayFactory = () => _sourceControlGatewayMock.Object,
                ChangesQuery = _changesQueryMock.Object,
                PublishEvent = _publishEventMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _sourceControlGatewayMock = new Mock<ISourceControlChangeGateway>(MockBehavior.Strict);
            _changesQueryMock = new Mock<IHandleQuery<GetReleaseChangesRequest, GetReleaseChangesResponse>>(MockBehavior.Strict);
            _publishEventMock = new Mock<IPublishEvent>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToStoreChanges_WhenChangesExist()
        {
            var command = new StoreSourceControlChangesCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            };
            var changes = Builder<SourceControlChange>.CreateListOfSize(5).Build();

            _changesQueryMock.Setup(c => c.Handle(It.Is<GetReleaseChangesRequest>(
                            r => r.IsBackground && r.ReleaseWindowId == command.ReleaseWindowId)))
                .Returns(new GetReleaseChangesResponse { Changes = changes });
            _sourceControlGatewayMock.Setup(x => x.StoreChanges(changes, command.ReleaseWindowId));
            _sourceControlGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _sourceControlGatewayMock.Verify(g => g.StoreChanges(
                It.IsAny<IEnumerable<SourceControlChange>>(), It.IsAny<Guid>()), Times.Once);
            _changesQueryMock.Verify(c => c.Handle(It.IsAny<GetReleaseChangesRequest>()), Times.Once);
            _publishEventMock.Verify(x => x.Publish(It.IsAny<IEvent>()), Times.Never);
        }
        [Test]
        public void Handle_ShouldNotCallGatewayToStoreChanges_WhenChangesNotExist()
        {
            var command = new StoreSourceControlChangesCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            };
            var changes = Enumerable.Empty<SourceControlChange>();

            _changesQueryMock.Setup(c => c.Handle(It.Is<GetReleaseChangesRequest>(
                            r => r.IsBackground && r.ReleaseWindowId == command.ReleaseWindowId)))
                .Returns(new GetReleaseChangesResponse { Changes = changes });
            _sourceControlGatewayMock.Setup(x => x.StoreChanges(changes, command.ReleaseWindowId));
            _sourceControlGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _changesQueryMock.Verify(c => c.Handle(It.IsAny<GetReleaseChangesRequest>()), Times.Once);
            _sourceControlGatewayMock.Verify(g => g.StoreChanges(
                It.IsAny<IEnumerable<SourceControlChange>>(), It.IsAny<Guid>()), Times.Never);
            _publishEventMock.Verify(x => x.Publish(It.IsAny<IEvent>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldPublishNotificationEventForUser_WhenDeploymentToolRequestFailed()
        {
            var command = new StoreSourceControlChangesCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            };

            _changesQueryMock.Setup(c =>c.Handle(It.Is<GetReleaseChangesRequest>(
                            r => r.IsBackground && r.ReleaseWindowId == command.ReleaseWindowId)))
                .Throws(new FailedToRetrieveSourceControlChangesException("Test"));
            _publishEventMock.Setup(x =>
                x.Publish(It.Is<NotificationOccuredForUserEvent>(ev =>
                    ev.Message == "Unable to attach Source Control changes to release window. Please request administrator for help"
                    && ev.Code == "FailedToRetrieveSourceControlChanges"
                ))).Returns((Task[]) null);

            Sut.Handle(command);

            _publishEventMock.Verify(x =>
                x.Publish(It.IsAny<NotificationOccuredForUserEvent>()), Times.Once);
        }
    }
}
