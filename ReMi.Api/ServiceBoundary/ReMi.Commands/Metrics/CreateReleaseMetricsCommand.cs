using System;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Metrics
{
    [Command("Create metrics for release window", CommandGroup.Metrics, IsBackground = true)]
    public class CreateReleaseMetricsCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindow={0}, CommandContext={1}]", ReleaseWindow, CommandContext);
        }
    }
}
