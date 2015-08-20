using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class TicketChangedEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }
        public List<ReleaseContentTicket> Tickets { get; set; }
        public Guid ReleaseWindowExternalId { get; set; }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("[Context={0}, Tickets={1}, ReleaseWindowExternalId={2}]", Context,
                Tickets.FormatElements(),
                ReleaseWindowExternalId);
        }

        public Guid ReleaseWindowId {
            get { return ReleaseWindowExternalId; }
        }
    }
}
