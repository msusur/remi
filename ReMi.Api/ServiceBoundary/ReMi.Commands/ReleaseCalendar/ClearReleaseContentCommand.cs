using ReMi.Contracts.Cqrs.Commands;
using System;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Clear release content", CommandGroup.ReleaseCalendar, IsBackground = true)]
    public class ClearReleaseContentCommand : ICommand
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
