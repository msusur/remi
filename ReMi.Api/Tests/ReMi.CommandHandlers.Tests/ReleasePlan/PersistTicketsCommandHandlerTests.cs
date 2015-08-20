using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class PersistTicketsCommandHandlerTests : TestClassFor<PersistTicketsCommandHandler>
    {
        private Mock<IReleaseContentGateway> _releaseContentGatewayMock;

        protected override PersistTicketsCommandHandler ConstructSystemUnderTest()
        {
            return new PersistTicketsCommandHandler
            {
                ReleaseContentGatewayFactory = () => _releaseContentGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseContentGatewayMock = new Mock<IReleaseContentGateway>(MockBehavior.Strict);
            _releaseContentGatewayMock.Setup(x => x.Dispose());

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToPersistTickets_WhenInvoked()
        {
            var command = new PersistTicketsCommand
            {
                Tickets = new List<ReleaseContentTicket> { new ReleaseContentTicket() },
                CommandContext = new CommandContext { UserId = Guid.NewGuid() },
                ReleaseWindowId = Guid.NewGuid()
            };

            _releaseContentGatewayMock.Setup(g => g.AddOrUpdateTickets(command.Tickets, command.CommandContext.UserId, command.ReleaseWindowId));
            
            Sut.Handle(command);

            _releaseContentGatewayMock.Verify(g => g.AddOrUpdateTickets(command.Tickets, command.CommandContext.UserId, command.ReleaseWindowId));
        }
    }
}
