using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Hide ticket", CommandGroup.ReleasePlan)]
    public class UpdateTicketToReleaseNotesRelationCommand : ICommand
    {
        public List<ReleaseContentTicket> Tickets { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public bool AutomatedDeploy { get; set; }

        public CommandContext CommandContext { get; set; }

        public override string ToString()
        {
            return
                String.Format(
                    "[Tickets={0}, ReleaseWindowId={1}, AutomatedDeploy={2}, CommandContext={3}]",
                    Tickets.FormatElements(), ReleaseWindowId, AutomatedDeploy, CommandContext);
        }
    }
}
