using ReMi.Common.WebApi.Notifications;
using System;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseExecution
{
    public class ReleaseDecisionChangedEvent : IEvent, INotificationFilterByReleaseWindowId
    {
        public EventContext Context { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public String ReleaseDecision { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseDecision={0}, ReleaseWindowId={1}, Context={2}]",
                ReleaseDecision, ReleaseWindowId, Context);
        }
    }
}
