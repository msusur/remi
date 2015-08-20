using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Manage release desicion according to qa status", CommandGroup.ReleasePlan)]
    public class CheckQaStatusCommand: ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, CommandContext={1}]", ReleaseWindowId, CommandContext);
        }
    }
}
