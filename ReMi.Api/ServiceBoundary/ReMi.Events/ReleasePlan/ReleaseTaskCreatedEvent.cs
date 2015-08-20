using System;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class ReleaseTaskCreatedEvent : IEvent, INotificationFilterByReleaseWindowId
    {
        public ReleaseTask ReleaseTask { get; set; }

        public EventContext Context { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseTask={0}, Context={1}]",
                ReleaseTask, Context);
        }

        public Guid ReleaseWindowId { get { return ReleaseTask.ReleaseWindowId; } }
    }
}
