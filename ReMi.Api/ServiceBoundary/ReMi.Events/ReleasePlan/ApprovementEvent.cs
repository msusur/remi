using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class ApprovementEvent : IEvent, INotificationFilterByReleaseWindowId
    {
        public EventContext Context { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public String Comment { get; set; }
        public Guid ApproverId { get; set; }

        public override string ToString()
        {
            return String.Format("[Context={0}, ReleaseWindowId={1}, Comment={2}, ApproverId={3}]",
                Context, ReleaseWindowId, Comment, ApproverId);
        }
    }
}
