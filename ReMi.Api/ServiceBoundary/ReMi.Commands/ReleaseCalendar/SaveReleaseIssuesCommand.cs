using System;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Save Release Issues", CommandGroup.ReleaseCalendar)]
    public class SaveReleaseIssuesCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindow={0}, Context={1}]", ReleaseWindow, CommandContext);
        }
    }
}
