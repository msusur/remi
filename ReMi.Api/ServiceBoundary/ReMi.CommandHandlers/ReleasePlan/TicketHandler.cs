using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class TicketHandler : IHandleCommand<UpdateTicketToReleaseNotesRelationCommand>, IHandleCommand<IncludeTicketsToReleaseNotesCommand>
    {
        public Func<IReleaseContentGateway> ReleaseContentGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(UpdateTicketToReleaseNotesRelationCommand command)
        {
            var accountId = command.CommandContext.UserId;

            using (var gateway = ReleaseContentGatewayFactory())
            {
                var tickets = gateway.GetTicketInformations(command.Tickets.Select(x => x.TicketId).ToList());
                var updatedTickets = new List<ReleaseContentTicket>();

                foreach (var ticket in command.Tickets)
                {
                    if (tickets.All(t => ticket.TicketId != t.TicketId))
                    {
                        if (command.AutomatedDeploy)
                        {
                            gateway.AddOrUpdateTickets(new[] {ticket}, accountId, command.ReleaseWindowId);
                        }
                        else
                        {
                            gateway.CreateTicket(ticket, accountId);
                        }
                    }
                    else
                    {
                        if(tickets.Any(t=>t.TicketId==ticket.TicketId&&t.IncludeToReleaseNotes==ticket.IncludeToReleaseNotes))
                        {
                            continue;
                        }
                        gateway.UpdateTicketReleaseNotesRelation(ticket, accountId);
                    }

                    updatedTickets.Add(ticket);
                }

                if (!updatedTickets.Any())
                {
                    return;
                }
                
                EventPublisher.Publish(new TicketChangedEvent
                {
                    Tickets = updatedTickets,
                    ReleaseWindowExternalId = command.ReleaseWindowId
                });
            }
        }

        public void Handle(IncludeTicketsToReleaseNotesCommand command)
        {
            var accountId = command.CommandContext.UserId;

            using (var gateway = ReleaseContentGatewayFactory())
            {
                foreach (var ticketId in command.TicketIds)
                {
                    gateway.UpdateTicketReleaseNotesRelation(
                        new ReleaseContentTicket {TicketId = ticketId, IncludeToReleaseNotes = true},
                        accountId);
                }
            }
        }
    }
}
