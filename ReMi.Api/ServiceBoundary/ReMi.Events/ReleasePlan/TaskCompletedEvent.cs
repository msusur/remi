using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class TaskCompletedEvent : IEvent, INotificationFilterByReleaseWindowId
    {
        public Guid ReleaseTaskExternalId { get; set; }
        public Guid ReleaseWindowExternalId { get; set; }
        public String AssigneeName { get; set; }

        public EventContext Context { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseTaskExteralId={0}, Context={1}, ReleaseWindowExternalId={2}, AssigneeName={3}]",
                ReleaseTaskExternalId, Context, ReleaseWindowExternalId, AssigneeName);
        }

        public Guid ReleaseWindowId {
            get { return ReleaseWindowExternalId; }
        }
    }
}
