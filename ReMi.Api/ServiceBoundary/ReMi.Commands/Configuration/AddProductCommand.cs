using System;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Configuration
{
    [Command("Add package", CommandGroup.Configuration)]
    public class AddProductCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public string Description { get; set; }

        public Guid ExternalId { get; set; }

        public ReleaseTrack ReleaseTrack { get; set; }

        public Boolean ChooseTicketsByDefault { get; set; }

        public Guid BusinessUnitId { get; set; }


        public override string ToString()
        {
            return String.Format("[Description={0}, ExternalId={1}, ReleaseTrack={2}, ChooseTicketsByDefault={3}, BusinessUnitId={4}]",
                Description, ExternalId, ReleaseTrack, ChooseTicketsByDefault, BusinessUnitId);
        }
    }
}
