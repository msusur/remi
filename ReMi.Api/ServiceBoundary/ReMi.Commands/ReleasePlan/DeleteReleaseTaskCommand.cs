using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Remove Task", CommandGroup.ReleaseTask)]
    public class DeleteReleaseTaskCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseTaskId { get; set; }

        public override string ToString()
        {
            return string.Format("[CommandContext = {0}, ReleaseTaskId = {1}]",
                CommandContext, ReleaseTaskId);
        }
    }
}
