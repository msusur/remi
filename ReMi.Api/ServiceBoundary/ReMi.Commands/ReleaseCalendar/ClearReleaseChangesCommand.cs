using ReMi.Contracts.Cqrs.Commands;
using System;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Clear release changes", CommandGroup.ReleaseCalendar, IsBackground = true)]
    public class ClearReleaseChangesCommand : ICommand
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
