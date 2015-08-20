using System;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Update Release Decision", CommandGroup.ReleaseCalendar, IsBackground = true)]
    public class UpdateReleaseDecisionCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseWindowId { get; set; }
        public ReleaseDecision ReleaseDecision { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindowId={0}, ReleaseDecision={1}, CommandContext={2}]",
                ReleaseWindowId, ReleaseDecision, CommandContext);
        }

    }
}
