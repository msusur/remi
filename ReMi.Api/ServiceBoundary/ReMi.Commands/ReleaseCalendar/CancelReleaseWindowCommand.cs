using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Cancel Release", CommandGroup.ReleaseCalendar)]
    public class CancelReleaseWindowCommand : ICommand
    {
        public Guid ExternalId { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindow.ExternalId = {0}]", ExternalId);
        }

        public CommandContext CommandContext { get; set; }
    }
}
