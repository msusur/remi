using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Acknowledge Release Task", CommandGroup.ReleaseTask, IsBackground = true)]
    public class ConfirmReleaseTaskReceiptCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseTaskId { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseTaskId={0}, CommandContext={1}]", ReleaseTaskId, CommandContext);
        }
    }
}
