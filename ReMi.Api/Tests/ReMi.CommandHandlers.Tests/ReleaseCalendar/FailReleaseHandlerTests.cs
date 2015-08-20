using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.Auth;
using ReMi.CommandHandlers.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;
using ReMi.Events.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class FailReleaseHandlerTests : TestClassFor<FailReleaseHandler>
    {
        private Mock<IAccountsBusinessLogic> _accountsBusinessLogicMock;
        private Mock<ISourceControlChangeGateway> _sourceControlChangesGatewayMock;
        private Mock<IReleaseContentGateway> _releaseContentGatewayMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IPublishEvent> _publishEventMock;

        protected override FailReleaseHandler ConstructSystemUnderTest()
        {
            return new FailReleaseHandler
            {
                AccountsBusinessLogic = _accountsBusinessLogicMock.Object,
                SourceControlChangesGatewayFactory = () => _sourceControlChangesGatewayMock.Object,
                ReleaseContentGatewayFactory = () => _releaseContentGatewayMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                PublishEvent = _publishEventMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountsBusinessLogicMock = new Mock<IAccountsBusinessLogic>();
            _sourceControlChangesGatewayMock = new Mock<ISourceControlChangeGateway>();
            _releaseContentGatewayMock = new Mock<IReleaseContentGateway>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _publishEventMock = new Mock<IPublishEvent>();

            _accountsBusinessLogicMock.Setup(x => x.SignSession(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Session());

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(FailedToAuthenticateException))]
        public void Handle_ShouldThrowException_WhenSignSessionFailed()
        {
            var command = ConstructCommand();

            _accountsBusinessLogicMock.Setup(x => x.SignSession(command.UserName, command.Password))
                .Returns((Session)null);

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldRemoveTickets_WhenInvoked()
        {
            var command = ConstructCommand();

            Sut.Handle(command);

            _releaseContentGatewayMock.Verify(x => x.RemoveTicketsFromRelease(command.ReleaseWindowId));
        }

        [Test]
        public void Handle_ShouldRemoveChangesFromRelease_WhenInvoked()
        {
            var command = ConstructCommand();

            _sourceControlChangesGatewayMock.Setup(x => x.RemoveChangesFromRelease(command.ReleaseWindowId));

            Sut.Handle(command);

            _sourceControlChangesGatewayMock.Verify(x => x.RemoveChangesFromRelease(command.ReleaseWindowId));
        }

        [Test]
        public void Handle_ShouldMarkReleaseAsFailed_WhenInvoked()
        {
            var command = ConstructCommand(issues: "issues");

            Sut.Handle(command);

            _releaseWindowGatewayMock.Verify(x => x.CloseFailedRelease(command.ReleaseWindowId, "issues"));
        }

        [Test]
        public void Handle_PublishEvents_WhenInvoked()
        {
            var command = ConstructCommand(issues: "issues");

            Sut.Handle(command);

            _publishEventMock.Verify(x => x.Publish(It.Is<ReleaseWindowClosedEvent>(ev => ev.IsFailed && ev.ReleaseWindowId == command.ReleaseWindowId)));

            _publishEventMock.Verify(x => x.Publish(It.Is<ReleaseStatusChangedEvent>(ev => ev.ReleaseWindowId == command.ReleaseWindowId && ev.ReleaseStatus == "Closed")));
        }

        private FailReleaseCommand ConstructCommand(Guid? externalId = null, string issues = null)
        {
            return new FailReleaseCommand
            {
                ReleaseWindowId = externalId ?? Guid.NewGuid(),
                Issues = issues,
                CommandContext = new CommandContext
                {
                    UserId = Guid.NewGuid()
                },
                UserName = "username",
                Password = "password"
            };
        }
    }
}
