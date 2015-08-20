using System;
using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Include Tickets to Release Notes", CommandGroup.ReleasePlan, IsBackground = true)]
    public class IncludeTicketsToReleaseNotesCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public List<Guid> TicketIds { get; set; }

        public override string ToString()
        {
            return String.Format("[CommandContext={0}, TicketIds={1}]", CommandContext,
                TicketIds.FormatElements());
        }
    }
}
