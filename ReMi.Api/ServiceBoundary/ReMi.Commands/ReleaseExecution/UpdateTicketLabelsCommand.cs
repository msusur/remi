using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseExecution
{
    [Command("Update Ticket Labels Comment", CommandGroup.ReleaseExecution, IsBackground = true)]
    public class UpdateTicketLabelsCommand : ICommand
    {
        public List<ReleaseContentTicket> Tickets { get; set; }
        public Guid PackageId { get; set; }
        public CommandContext CommandContext { get; set; }

        public override string ToString()
        {
            return String.Format("[Tickets={0}, CommandContext={1}, PackageId={2}]",
                Tickets.FormatElements(), CommandContext, PackageId);
        }
    }
}
