using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class CreateReleaseTaskCommandHandlerTests : TestClassFor<CreateReleaseTaskCommandHandler>
    {
        private Mock<IReleaseTaskGateway> _releaseTaskGatewayFactoryMock;
        private Mock<IAccountsGateway> _accountGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;

        protected override CreateReleaseTaskCommandHandler ConstructSystemUnderTest()
        {
            return new CreateReleaseTaskCommandHandler
            {
                PublishEvent = _eventPublisherMock.Object,
                ReleaseTaskGatewayFactory = () => _releaseTaskGatewayFactoryMock.Object,
                AccountGatewayFactory = () => _accountGatewayMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object,
            };
        }

        protected override void TestInitialize()
        {
            _releaseTaskGatewayFactoryMock = new Mock<IReleaseTaskGateway>();
            _eventPublisherMock = new Mock<IPublishEvent>();
            _accountGatewayMock = new Mock<IAccountsGateway>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldPopulateCreatedBy_WhenInvoked()
        {
            var assignee = CreateAccount();
            var releaseTask = new ReleaseTask { AssigneeExternalId = assignee.ExternalId };

            _accountGatewayMock.Setup(x => x.GetAccount(assignee.ExternalId, true))
                .Returns(assignee);

            Sut.Handle(new CreateReleaseTaskCommand
            {
                CommandContext = new CommandContext { UserId = assignee.ExternalId, UserName = assignee.FullName },
                Assignee = assignee,
                ReleaseTask = releaseTask,
            });

            Assert.AreEqual(assignee.ExternalId, releaseTask.CreatedByExternalId);
            Assert.AreEqual(assignee.FullName, releaseTask.CreatedBy);
        }

        [Test]
        public void Handle_ShouldCallGatewayMethodAndDisposeIt_WhenCallGateway()
        {
            var assignee = CreateAccount();
            var releaseTask = new ReleaseTask { AssigneeExternalId = assignee.ExternalId };
            _accountGatewayMock.Setup(x => x.GetAccount(assignee.ExternalId, true))
                .Returns(assignee);

            Sut.Handle(new CreateReleaseTaskCommand
            {
                CommandContext = new CommandContext { UserId = assignee.ExternalId },
                Assignee = assignee,
                ReleaseTask = releaseTask,
            });

            _releaseTaskGatewayFactoryMock.Verify(x => x.CreateReleaseTask(releaseTask));
            _releaseTaskGatewayFactoryMock.Verify(x => x.Dispose());
        }

        [Test]
        public void Handle_ShouldpublishCreateHelpDeskTaskCommand_WhenCreateHelpDeskTicketOptionTurnedOn()
        {
            var assignee = CreateAccount();
            var releaseTask = new ReleaseTask { CreateHelpDeskTicket = true, AssigneeExternalId = assignee.ExternalId };
            _accountGatewayMock.Setup(x => x.GetAccount(assignee.ExternalId, true))
                .Returns(assignee);

            Sut.Handle(new CreateReleaseTaskCommand
            {
                CommandContext = new CommandContext { UserId = assignee.ExternalId },
                Assignee = assignee,
                ReleaseTask = releaseTask,
            });

            _commandDispatcherMock.Verify(x => x.Send(It.Is<CreateHelpDeskTaskCommand>(e => e.ReleaseTask == releaseTask)));
        }

        [Test]
        public void Handle_ShouldPublishNotificationEvent_WhenTaskHasNotHelpDeskTicket()
        {
            var assignee = CreateAccount();
            var releaseTask = new ReleaseTask { CreateHelpDeskTicket = true, AssigneeExternalId = assignee.ExternalId };
            _accountGatewayMock.Setup(x => x.GetAccount(assignee.ExternalId, true))
                .Returns(assignee);

            Sut.Handle(new CreateReleaseTaskCommand
            {
                CommandContext = new CommandContext { UserId = assignee.ExternalId },
                Assignee = assignee,
                ReleaseTask = releaseTask,
            });

            _eventPublisherMock.Verify(x => x.Publish(It.Is<ReleaseTaskCreatedEvent>(e => e.ReleaseTask == releaseTask)));
        }


        private Account CreateAccount()
        {
            return Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.FullName, RandomData.RandomString(20))
                .With(o => o.Email, RandomData.RandomEmail())
                .Build();
        }


    }
}
