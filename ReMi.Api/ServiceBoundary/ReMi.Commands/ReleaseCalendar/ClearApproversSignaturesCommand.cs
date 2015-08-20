using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Clear approvers signatures", CommandGroup.ReleaseCalendar, IsBackground = true)]
    public class ClearApproversSignaturesCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return string.Format("[CommandContext={0}, ReleaseWindowId={1}]",
                CommandContext, ReleaseWindowId);
        }
    }
}
