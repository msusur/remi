using System.Collections.Generic;
using AutoMapper;
using ReMi.Commands.ReleaseExecution;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Services.ReleaseContent;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class UpdateTicketLabelsCommandHandler : IHandleCommand<UpdateTicketLabelsCommand>
    {
        public IReleaseContent ReleaseContent { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public void Handle(UpdateTicketLabelsCommand command)
        {
            ReleaseContent.UpdateTicket(MappingEngine.Map<
                IEnumerable<BusinessEntities.ReleasePlan.ReleaseContentTicket>,
                IEnumerable<Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket>>(
                    command.Tickets), command.PackageId);
        }
    }
}
