using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Persist Tickets", CommandGroup.ReleasePlan, IsBackground = true)]
    public class PersistTicketsCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public IEnumerable<ReleaseContentTicket> Tickets { get; set; }
 
        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[CommandContext={0}, Tickets.Count={1}, ReleaseWindowId={2}]", CommandContext, Tickets.Count(), ReleaseWindowId);
        }
    }
}
