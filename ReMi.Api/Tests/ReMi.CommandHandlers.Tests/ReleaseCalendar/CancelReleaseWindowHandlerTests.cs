using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class CancelReleaseWindowHandlerTests : TestClassFor<CancelReleaseWindowHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IReleaseParticipantGateway> _releaseParticipantGatewayMock;
        private Mock<IPublishEvent> _publishEventMock;

        private Mock<IMappingEngine> _mappingEngineMock;

        protected override CancelReleaseWindowHandler ConstructSystemUnderTest()
        {
            return new CancelReleaseWindowHandler
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                ReleaseParticipantGatewayFactory = () => _releaseParticipantGatewayMock.Object,
                Mapper = _mappingEngineMock.Object,
                PublishEvent = _publishEventMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _mappingEngineMock = new Mock<IMappingEngine>();
            _publishEventMock = new Mock<IPublishEvent>();
            _releaseParticipantGatewayMock = new Mock<IReleaseParticipantGateway>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(ReleaseNotFoundException))]
        public void Handle_ShouldThrowReleaseNotFoundException_WhenReleaseNotExists()
        {
            var command = CreateCommand();

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldCancelReleaseWindow_WhenInvoked()
        {
            var command = CreateCommand();

            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId, command.ExternalId)
                .Build();

            var account = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _releaseWindowGatewayMock.Setup(g => g.GetByExternalId(releaseWindow.ExternalId, It.IsAny<bool>(), It.IsAny<bool>())).Returns(releaseWindow);

            _releaseParticipantGatewayMock.Setup(rp => rp.GetReleaseParticipants(releaseWindow.ExternalId))
                .Returns(new List<ReleaseParticipant> { new ReleaseParticipant { Account = account } });

            Sut.Handle(command);

            _releaseWindowGatewayMock.Verify(g => g.Cancel(releaseWindow), "Should call Cancel method in ReleaseWindowGateway");
        }

        [Test]
        public void Handle_ShouldRequestForReleaseTeam_WhenInvoked()
        {
            var command = CreateCommand();

            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId, command.ExternalId)
                .Build();

            var account = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _releaseWindowGatewayMock.Setup(g => g.GetByExternalId(releaseWindow.ExternalId, It.IsAny<bool>(), It.IsAny<bool>())).Returns(releaseWindow);

            _releaseParticipantGatewayMock.Setup(rp => rp.GetReleaseParticipants(releaseWindow.ExternalId))
                .Returns(new List<ReleaseParticipant> { new ReleaseParticipant { Account = account } });

            Sut.Handle(command);

            _releaseParticipantGatewayMock.Verify(rp => rp.GetReleaseParticipants(releaseWindow.ExternalId));
        }

        [Test]
        public void Handle_ShouldSendReleaseWindowCanceledEvent_WhenInvoked()
        {
            var command = CreateCommand();

            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId, command.ExternalId)
                .Build();

            var account = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            var otherAccount = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _releaseWindowGatewayMock.Setup(g => g.GetByExternalId(releaseWindow.ExternalId, false, It.IsAny<bool>())).Returns(releaseWindow);

            _releaseParticipantGatewayMock.Setup(rp => rp.GetReleaseParticipants(releaseWindow.ExternalId))
                .Returns(new List<ReleaseParticipant> { new ReleaseParticipant { Account = account } });

            Sut.Handle(command);

            _publishEventMock.Verify(
                p =>
                    p.Publish(
                        It.Is<ReleaseWindowCanceledEvent>(
                            e =>
                                e.ReleaseWindow == releaseWindow &&
                                e.Participants.First().ExternalId == account.ExternalId)));
        }

        [Test]
        public void Handle_ShouldSendReleaseWindowCanceledEventToAllInvolvedAccounts_WhenInvoked()
        {
            var command = CreateCommand();

            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId, command.ExternalId)
                .Build();

            var account1 = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            var account2 = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            var account3 = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _releaseWindowGatewayMock.Setup(g => g.GetByExternalId(releaseWindow.ExternalId, It.IsAny<bool>(), It.IsAny<bool>())).Returns(releaseWindow);

            _releaseParticipantGatewayMock.Setup(rp => rp.GetReleaseParticipants(releaseWindow.ExternalId))
                .Returns(new List<ReleaseParticipant> { new ReleaseParticipant { Account = account1 } });

            Sut.Handle(command);

            _publishEventMock.Verify(
                p => p.Publish(It.Is<ReleaseWindowCanceledEvent>(e =>
                    e.ReleaseWindow == releaseWindow
                    && e.Participants.Any(x => x.ExternalId == account1.ExternalId)
                )));
        }

        private CancelReleaseWindowCommand CreateCommand(Guid? releaseWindowId = null)
        {
            var account = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            return new CancelReleaseWindowCommand
            {
                CommandContext = new CommandContext
                {
                    UserId = Guid.NewGuid()
                },
                ExternalId = releaseWindowId ?? Guid.NewGuid()
            };
        }
    }
}
