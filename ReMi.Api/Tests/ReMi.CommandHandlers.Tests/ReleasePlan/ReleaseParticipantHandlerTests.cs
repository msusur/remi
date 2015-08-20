using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.Auth;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.Exceptions;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class ReleaseParticipantHandlerTests : TestClassFor<ReleaseParticipantHandler>
    {
        private Mock<IReleaseParticipantGateway> _releaseParticipantGatewayMock;
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<ICommandDispatcher> _commandDispatcher;

        private readonly Account _account = new Account
        {
            Email = RandomData.RandomEmail(),
            FullName = RandomData.RandomString(10),
            Role = new Role { Name = new[] { "Admin", "BasicUser", "NotAuthenticated", "ProductOwner" }[RandomData.RandomInt(0, 3)] }
        };

        private readonly Guid _authorId = Guid.NewGuid();
        private List<Account> _accounts;

        private ReleaseParticipant _releaseParticipant;
        private List<ReleaseParticipant> _releaseParticipants;


        protected override ReleaseParticipantHandler ConstructSystemUnderTest()
        {
            return new ReleaseParticipantHandler
            {
                ReleaseParticipantGatewayFactory = () => _releaseParticipantGatewayMock.Object,
                AccountsGatewayFactory = () => _accountsGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object,
                CommandDispatcher = _commandDispatcher.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseParticipantGatewayMock =
                new Mock<IReleaseParticipantGateway>();
            _accountsGatewayMock = new Mock<IAccountsGateway>();
            _eventPublisherMock = new Mock<IPublishEvent>();
            _commandDispatcher = new Mock<ICommandDispatcher>();

            _accounts = new List<Account> { _account };
            _releaseParticipant = new ReleaseParticipant
            {
                Account = _account,
                ReleaseWindowId = Guid.NewGuid(),
                ReleaseParticipantId = Guid.NewGuid()
            };

            _releaseParticipants = new List<ReleaseParticipant> { _releaseParticipant };

            _releaseParticipantGatewayMock.Setup(rp => rp.AddReleaseParticipants(_releaseParticipants));
            _releaseParticipantGatewayMock.Setup(rp => rp.RemoveReleaseParticipant(_releaseParticipant));
            _releaseParticipantGatewayMock.Setup(rp => rp.GetReleaseParticipants(_releaseParticipant.ReleaseWindowId))
                .Returns(new List<ReleaseParticipant> { new ReleaseParticipant { Account = _account } });

            _accountsGatewayMock.Setup(account => account.GetProductOwners(new[] { "team" })).Returns(_accounts);
            _accountsGatewayMock.Setup(account => account.GetAccounts(It.IsAny<IEnumerable<Guid>>()))
                .Returns(_accounts);

            _releaseParticipantGatewayMock.Setup(
                rp => rp.ApproveReleaseParticipation(_releaseParticipant.ReleaseParticipantId));
            _releaseParticipantGatewayMock.Setup(
                rp => rp.ClearParticipationApprovements(_releaseParticipant.ReleaseWindowId, _authorId));
            _accountsGatewayMock.Setup(acc => acc.CreateNotExistingReleaseParticipants(_releaseParticipants));

            base.TestInitialize();
        }

        [Test]
        public void AddReleaseParticipantTest()
        {
            Sut.Handle(new AddReleaseParticipantCommand
            {
                Participants = _releaseParticipants
            });

            _accountsGatewayMock.Verify(acc => acc.CreateNotExistingReleaseParticipants(_releaseParticipants));
            _releaseParticipantGatewayMock.Verify(rp => rp.AddReleaseParticipants(_releaseParticipants));
            _commandDispatcher.Verify(s => s.Send(It.IsAny<AssociateAccountsWithProductCommand>()));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseParticipantsAddedEvent>(
                            x =>
                                x.Participants == _releaseParticipants &&
                                x.ReleaseWindowId == _releaseParticipants[0].ReleaseWindowId)));
        }


        [Test]
        [ExpectedException(typeof(AccountIsBlockedException))]
        public void AddReleaseParticipant_ShouldThrowException_WhenInvoked()
        {
            var accounts = new List<Account> { new Account { ExternalId = _account.ExternalId, IsBlocked = true } };

            _accountsGatewayMock.Setup(account => account.GetAccounts(It.IsAny<IEnumerable<Guid>>()))
                .Returns(accounts);

            Sut.Handle(new AddReleaseParticipantCommand
            {
                Participants = _releaseParticipants
            });
        }

        [Test]
        public void AddReleaseParticipant_EmptyEntity_ShouldNotCallGateways()
        {
            Sut.Handle(new AddReleaseParticipantCommand
            {
                Participants = new List<ReleaseParticipant>()
            });

            _accountsGatewayMock.Verify(acc => acc.CreateNotExistingReleaseParticipants(_releaseParticipants), Times.Never);
            _releaseParticipantGatewayMock.Verify(rp => rp.AddReleaseParticipants(_releaseParticipants), Times.Never);
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseParticipantsAddedEvent>(
                            x =>
                                x.Participants == _releaseParticipants &&
                                x.ReleaseWindowId == _releaseParticipants[0].ReleaseWindowId)), Times.Never);
        }

        [Test]
        public void RemoveReleaseParticipantTest()
        {
            Sut.Handle(new RemoveReleaseParticipantCommand
            {
                Participant = _releaseParticipant
            });

            _releaseParticipantGatewayMock.Setup(rp => rp.RemoveReleaseParticipant(_releaseParticipant));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseParticipantRemovedEvent>(
                            x =>
                                x.Participant == _releaseParticipant &&
                                x.ReleaseWindowId == _releaseParticipant.ReleaseWindowId)));
        }
    }
}
