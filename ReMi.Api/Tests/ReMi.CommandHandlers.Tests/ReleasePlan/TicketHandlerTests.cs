using System;
using System.Collections.Generic;
using System.Linq;
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
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class TicketHandlerTests : TestClassFor<TicketHandler>
    {
        private UpdateTicketToReleaseNotesRelationCommand _updateTicketToReleaseNotesRelationCommand;
        private IncludeTicketsToReleaseNotesCommand _includeTicketsToReleaseNotesCommand;
        private Mock<IReleaseContentGateway> _releaseContentGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private List<ReleaseContentTicket> _tickets;
        private BusinessAccount _account;

        protected override TicketHandler ConstructSystemUnderTest()
        {
            return new TicketHandler
            {
                ReleaseContentGatewayFactory = () => _releaseContentGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _account = new BusinessAccount
            {
                ExternalId = Guid.NewGuid()
            };
            _eventPublisherMock = new Mock<IPublishEvent>();
            _releaseContentGatewayMock = new Mock<IReleaseContentGateway>();
            _includeTicketsToReleaseNotesCommand = new IncludeTicketsToReleaseNotesCommand
            {
                TicketIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
                CommandContext =
                    new CommandContext { UserId = _account.ExternalId }
            };
            _updateTicketToReleaseNotesRelationCommand = new UpdateTicketToReleaseNotesRelationCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                Tickets =
                    new List<ReleaseContentTicket>
                    {
                        new ReleaseContentTicket { TicketId =  Guid.NewGuid(), IncludeToReleaseNotes = false },
                        new ReleaseContentTicket { TicketId =  Guid.NewGuid(), IncludeToReleaseNotes = true },
                        new ReleaseContentTicket { TicketId =  Guid.NewGuid(), IncludeToReleaseNotes = true }
                    },
                CommandContext = new CommandContext { UserId = _account.ExternalId }
            };
            _tickets = new List<ReleaseContentTicket>
            {
                _updateTicketToReleaseNotesRelationCommand.Tickets[0],
                new ReleaseContentTicket
                {
                    TicketId = _updateTicketToReleaseNotesRelationCommand.Tickets[1].TicketId,
                    IncludeToReleaseNotes =
                        !_updateTicketToReleaseNotesRelationCommand.Tickets[1].IncludeToReleaseNotes
                }
            };
            _releaseContentGatewayMock.Setup(
                j =>
                    j.GetTicketInformations(
                        It.Is<List<Guid>>(
                            l =>
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[0].TicketId) &&
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[1].TicketId) &&
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[2].TicketId))))
                .Returns(_tickets);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCorrectlyHandleUpdateCommand()
        {
            Sut.Handle(_updateTicketToReleaseNotesRelationCommand);

            _releaseContentGatewayMock.Verify(
                j =>
                    j.GetTicketInformations(
                        It.Is<List<Guid>>(
                            l =>
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[0].TicketId) &&
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[1].TicketId) &&
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[2].TicketId))));
            _releaseContentGatewayMock.Verify(
                j =>
                    j.UpdateTicketReleaseNotesRelation(
                        _updateTicketToReleaseNotesRelationCommand.Tickets[1],
                        _updateTicketToReleaseNotesRelationCommand.CommandContext.UserId));
            _releaseContentGatewayMock.Verify(
                j =>
                    j.CreateTicket(_updateTicketToReleaseNotesRelationCommand.Tickets[2],
                        _updateTicketToReleaseNotesRelationCommand.CommandContext.UserId));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<TicketChangedEvent>(
                            j =>
                                j.ReleaseWindowExternalId ==
                                _updateTicketToReleaseNotesRelationCommand.ReleaseWindowId &&
                                j.Tickets.Count == 2 &&
                                j.Tickets.Any(
                                    t =>
                                        t.TicketId ==
                                        _updateTicketToReleaseNotesRelationCommand.Tickets[1].TicketId) &&
                                j.Tickets.Any(
                                    t =>
                                        t.TicketId ==
                                        _updateTicketToReleaseNotesRelationCommand.Tickets[2].TicketId))));
        }

        [Test]
        public void Handle_ShouldNorRaiseEventWhentNoAccountsWereUpdated()
        {
            _releaseContentGatewayMock.Setup(
                j =>
                    j.GetTicketInformations(
                        It.Is<List<Guid>>(
                            l =>
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[0].TicketId) &&
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[1].TicketId) &&
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[2].TicketId))))
                .Returns(_updateTicketToReleaseNotesRelationCommand.Tickets);

            Sut.Handle(_updateTicketToReleaseNotesRelationCommand);

            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<TicketChangedEvent>(
                            j =>
                                j.ReleaseWindowExternalId ==
                                _updateTicketToReleaseNotesRelationCommand.ReleaseWindowId &&
                                j.Tickets.Count == 2 &&
                                j.Tickets.Any(
                                    t =>
                                        t.TicketId ==
                                        _updateTicketToReleaseNotesRelationCommand.Tickets[1].TicketId) &&
                                j.Tickets.Any(
                                    t =>
                                        t.TicketId ==
                                        _updateTicketToReleaseNotesRelationCommand.Tickets[2].TicketId))),
                Times.Never);
        }

        [Test]
        public void Handle_ShouldIncludeAllExcludedTicketsToReleaseNotes()
        {
            Sut.Handle(_includeTicketsToReleaseNotesCommand);

            _releaseContentGatewayMock.Verify(
                j =>
                    j.UpdateTicketReleaseNotesRelation(
                        It.Is<ReleaseContentTicket>(
                            t =>
                                t.IncludeToReleaseNotes &&
                                t.TicketId == _includeTicketsToReleaseNotesCommand.TicketIds[0]),
                        _includeTicketsToReleaseNotesCommand.CommandContext.UserId));
            _releaseContentGatewayMock.Verify(
                j =>
                    j.UpdateTicketReleaseNotesRelation(
                        It.Is<ReleaseContentTicket>(
                            t =>
                                t.IncludeToReleaseNotes &&
                                t.TicketId == _includeTicketsToReleaseNotesCommand.TicketIds[1]),
                        _includeTicketsToReleaseNotesCommand.CommandContext.UserId));
        }

        [Test]
        public void Handle_ShouldCorrectlyHandleUpdateCommand_DuringAutomatedDeploy()
        {
            _updateTicketToReleaseNotesRelationCommand.AutomatedDeploy = true;
            Sut.Handle(_updateTicketToReleaseNotesRelationCommand);

            _releaseContentGatewayMock.Verify(
                j =>
                    j.GetTicketInformations(
                        It.Is<List<Guid>>(
                            l =>
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[0].TicketId) &&
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[1].TicketId) &&
                                l.Contains(_updateTicketToReleaseNotesRelationCommand.Tickets[2].TicketId))));
            _releaseContentGatewayMock.Verify(
                j =>
                    j.UpdateTicketReleaseNotesRelation(
                        _updateTicketToReleaseNotesRelationCommand.Tickets[1],
                        _updateTicketToReleaseNotesRelationCommand.CommandContext.UserId));
            _releaseContentGatewayMock.Verify(
                j =>
                    j.AddOrUpdateTickets(It.IsAny<IEnumerable<ReleaseContentTicket>>(),
                        _updateTicketToReleaseNotesRelationCommand.CommandContext.UserId,
                        _updateTicketToReleaseNotesRelationCommand.ReleaseWindowId));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<TicketChangedEvent>(
                            j =>
                                j.ReleaseWindowExternalId ==
                                _updateTicketToReleaseNotesRelationCommand.ReleaseWindowId &&
                                j.Tickets.Count == 2 &&
                                j.Tickets.Any(
                                    t =>
                                        t.TicketId ==
                                        _updateTicketToReleaseNotesRelationCommand.Tickets[1].TicketId) &&
                                j.Tickets.Any(
                                    t =>
                                        t.TicketId ==
                                        _updateTicketToReleaseNotesRelationCommand.Tickets[2].TicketId))));
        }

    }
}
