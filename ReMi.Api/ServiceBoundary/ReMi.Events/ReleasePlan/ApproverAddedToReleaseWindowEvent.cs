using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;
using System;

namespace ReMi.Events.ReleasePlan
{
    public class ApproverAddedToReleaseWindowEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }

        public ReleaseApprover Approver { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return string.Format("[ReleaseWindowId={0}, Approver={1}, Context={2}]",
                ReleaseWindowId, Approver, Context);
        }
    }
}
