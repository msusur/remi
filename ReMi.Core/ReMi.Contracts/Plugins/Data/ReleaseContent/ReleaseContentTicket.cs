using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.Contracts.Plugins.Data.ReleaseContent
{
    public class ReleaseContentTicket
    {
        public string TicketName { get; set; }
        public string TicketDescription { get; set; }
        public string Assignee { get; set; }

        public Guid TicketId { get; set; }

        public string TicketUrl { get; set; }

        public IEnumerable<ReleaseContentTicket> SubTickets { get; set; }

        public override string ToString()
        {
            return String.Format("[TicketName={0},TicketDescription={1}, Assignee={2}, TicketId={3}, TicketUrl={4}, SubTickets={5}]",
                TicketName, TicketDescription, Assignee, TicketId, TicketUrl, SubTickets);
        }
    }
}
