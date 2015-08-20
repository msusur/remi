using System;
using ReMi.Common.Constants.ReleaseCalendar;

namespace ReMi.BusinessEntities.Products
{
    public class ProductView
    {
        public string Name { get; set; }

        public Guid ExternalId { get; set; }

        public ReleaseTrack ReleaseTrack { get; set; }

        public Boolean ChooseTicketsByDefault { get; set; }

        public bool IsDefault { get; set; }

        public string BusinessUnit { get; set; }

        public override string ToString()
        {
            return
                string.Format("[Name={0}, ExternalId={1}, IsDefault={2}, ReleaseTrack={3}, ChooseTicketsByDefault={4}, BusinessUnit={4}]",
                    Name, ExternalId, IsDefault, ReleaseTrack, ChooseTicketsByDefault, BusinessUnit);
        }

    }
}
