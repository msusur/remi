using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Complete Task", CommandGroup.ReleaseTask)]
    public class CompleteReleaseTaskCommand : ICommand
    {
        public Guid ReleaseTaskExtetnalId { get; set; }

        public CommandContext CommandContext { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseTaskExternalId={0}, CommandContext={1}]", ReleaseTaskExtetnalId,
                CommandContext);
        }
    }
}
