using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class ApproverRemovedFromReleaseEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }
        
        public Guid ApproverId { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return string.Format("[ReleaseWindowId={0}, AccountId={1}, Context={2}]",
                ReleaseWindowId, ApproverId, Context);
        }
    }
}
