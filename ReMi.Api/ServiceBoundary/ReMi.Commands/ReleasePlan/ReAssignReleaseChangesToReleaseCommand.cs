using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Re-assign release changes to release", CommandGroup.ReleasePlan)]
    public class ReAssignReleaseChangesToReleaseCommand : ICommand
    {
        public Guid ReleaseWindowId { get; set; }

        public CommandContext CommandContext { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindowId={0}, CommandContext={1}]",
                ReleaseWindowId, CommandContext);
        }
    }
}
