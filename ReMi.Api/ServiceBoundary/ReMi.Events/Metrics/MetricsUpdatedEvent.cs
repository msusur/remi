using System;
using ReMi.BusinessEntities.Metrics;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.Metrics
{
    public class MetricsUpdatedEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public Metric Metric { get; set; }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, Metric={1}, Context={2}]",
                ReleaseWindowId, Metric, Context);
        }
    }
}
