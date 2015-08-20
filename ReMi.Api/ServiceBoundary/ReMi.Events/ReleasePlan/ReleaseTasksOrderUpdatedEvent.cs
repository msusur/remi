using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class ReleaseTasksOrderUpdatedEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public Guid RequestorId { get { return Context.UserId; } }
    }
}
