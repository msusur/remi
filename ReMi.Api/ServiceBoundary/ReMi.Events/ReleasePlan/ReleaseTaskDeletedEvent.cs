using System;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class ReleaseTaskDeletedEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }

        public ReleaseTask ReleaseTask { get; set; }

        public Guid ReleaseWindowId { get { return ReleaseTask.ReleaseWindowId; } }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("[ReleaseTask={0}]", ReleaseTask);
        }
    }
}
