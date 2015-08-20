using System;
using ReMi.Common.Constants.ReleasePlan;

namespace ReMi.BusinessEntities.ReleasePlan
{
    public class ReleaseContentTicket
    {
        public string TicketName { get; set; }
        public string TicketDescription { get; set; }
        public string Assignee { get; set; }

        public Guid TicketId { get; set; }
        public string Comment { get; set; }
        public TicketRisk Risk { get; set; }
        public Guid LastChangedByAccount { get; set; }
        public bool IncludeToReleaseNotes { get; set; }

        public string TicketUrl { get; set; }

        public override string ToString()
        {
            return String.Format("[TicketName={0},TicketDescription={1}, Assignee={2}, Comment={3}, TicketId={4}, LastChangedByAccount={5}, TicketUrl={6}]",
                TicketName, TicketDescription, Assignee, Comment, TicketId, LastChangedByAccount, TicketUrl);
        }
    }
}
