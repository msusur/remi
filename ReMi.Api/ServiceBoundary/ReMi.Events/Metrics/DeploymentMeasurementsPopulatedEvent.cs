using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;
using System;

namespace ReMi.Events.Metrics
{
    public class DeploymentMeasurementsPopulatedEvent : IEvent, INotificationFilterByReleaseWindowId
    {
        public EventContext Context { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, Context={1}]",
                ReleaseWindowId, Context);
        }
    }
}
