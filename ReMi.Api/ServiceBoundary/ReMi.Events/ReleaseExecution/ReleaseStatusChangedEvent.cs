using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseExecution
{
    public class ReleaseStatusChangedEvent : IEvent, INotificationFilterByReleaseWindowId
    {
        public EventContext Context { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public String ReleaseStatus { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseStatus={0}, ReleaseWindowGuid={1}, Context={2}]",
                ReleaseStatus, ReleaseWindowId, Context);
        }
    }
}
