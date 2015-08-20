using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Create Checklist", CommandGroup.ReleasePlan, IsBackground = true)]
    public class CreateCheckListCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return string.Format("[CommandContext = {0}, ReleaseWindowId = {1}]",
                CommandContext, ReleaseWindowId);
        }
    }
}
