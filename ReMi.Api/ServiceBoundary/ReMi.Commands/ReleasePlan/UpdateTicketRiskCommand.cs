using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Update Ticket Risk", CommandGroup.ReleasePlan)]
    public class UpdateTicketRiskCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid TicketId { get; set; }
        public string TicketKey { get; set; }
        public bool IncludeToReleaseNotes { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public string Risk { get; set; }

        public override string ToString()
        {
            return String.Format("[TicketId={0}, TicketKey={1}, Risk={2}, IncludeToReleaseNotes={3}]",
                TicketId, TicketKey, Risk, IncludeToReleaseNotes);
        }
    }
}
