using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class UpdateTicketCommentCommandHandler : IHandleCommand<UpdateTicketCommentCommand>
    {
        public Func<IReleaseContentGateway> ReleaseContentGateway { get; set; }
        public IMappingEngine Mapper { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(UpdateTicketCommentCommand command)
        {
            using (var gateway = ReleaseContentGateway())
            {
                var ticket = Mapper.Map<UpdateTicketCommentCommand, ReleaseContentTicket>(command);
                gateway.AddOrUpdateTicketComment(ticket);
                ticket = gateway.GetTicketInformations(new[] {ticket.TicketId}).First();
                EventPublisher.Publish(new TicketChangedEvent
                {
                    ReleaseWindowExternalId = command.ReleaseWindowId,
                    Tickets = new List<ReleaseContentTicket> { ticket }
                });
            }
        }
    }
}
