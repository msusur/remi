using System;
using ReMi.Common.Constants.ReleaseCalendar;

namespace ReMi.BusinessEntities.Products
{
    public class Product
    {
        public string Description { get; set; }

        public Guid ExternalId { get; set; }

        public ReleaseTrack ReleaseTrack { get; set; }

        public Boolean ChooseTicketsByDefault { get; set; }

        public BusinessUnit BusinessUnit { get; set; }

        public override string ToString()
        {
            return String.Format("[Description={0}, ExternalId={1}, ReleaseTrack={2}, ChooseTicketsByDefault={3}, BusinessUnit={4}]",
                Description, ExternalId,
                ReleaseTrack, ChooseTicketsByDefault, BusinessUnit);
        }
    }
}
