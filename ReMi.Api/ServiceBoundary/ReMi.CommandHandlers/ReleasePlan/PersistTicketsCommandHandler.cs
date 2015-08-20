using System;
using System.Linq;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class PersistTicketsCommandHandler : IHandleCommand<PersistTicketsCommand>
    {
        public Func<IReleaseContentGateway> ReleaseContentGatewayFactory { get; set; }

        public void Handle(PersistTicketsCommand command)
        {
            if (command.Tickets == null || !command.Tickets.Any())
                return;

            using (var gateway = ReleaseContentGatewayFactory())
            {
                gateway.AddOrUpdateTickets(command.Tickets, command.CommandContext.UserId, command.ReleaseWindowId);
            }
        }
    }
}
