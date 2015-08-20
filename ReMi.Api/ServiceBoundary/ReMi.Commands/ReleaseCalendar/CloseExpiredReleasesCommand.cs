using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Close expired releases", CommandGroup.ReleaseCalendar)]
    public class CloseExpiredReleasesCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public override string ToString()
        {
            return String.Format("[CommandContext={0}]", CommandContext);
        }
    }
}
