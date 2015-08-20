using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseCalendar
{
    public class ReleaseIssuesUpdatedEvent : IEvent, INotificationFilterByReleaseWindowId
    {
        public EventContext Context { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public String Issues { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, Context={1}, Issues={2}]", ReleaseWindowId, Context, Issues);
        }
    }
}
