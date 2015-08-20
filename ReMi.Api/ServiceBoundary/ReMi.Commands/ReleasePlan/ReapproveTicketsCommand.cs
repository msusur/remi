using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Reapproving release content", CommandGroup.ReleasePlan)]
    public class ReapproveTicketsCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[CommandContext={0}, ReleaseWindowId={1}]", CommandContext, ReleaseWindowId);
        }
    }
}
