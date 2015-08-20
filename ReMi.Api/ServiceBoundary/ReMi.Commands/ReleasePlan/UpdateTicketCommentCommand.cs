using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Update Ticket Comment", CommandGroup.ReleasePlan)]
    public class UpdateTicketCommentCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid TicketId { get; set; }
        public string TicketKey { get; set; }
        public bool IncludeToReleaseNotes { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public string Comment { get; set; }

        public override string ToString()
        {
            return String.Format("[TicketId={0}, TicketKey={1}, Comment={2}, IncludeToReleaseNptes={3}]",
                TicketId, TicketKey, Comment, IncludeToReleaseNotes);
        }
    }
}
