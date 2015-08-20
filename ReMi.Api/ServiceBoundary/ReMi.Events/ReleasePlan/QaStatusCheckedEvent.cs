using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ContinuousDelivery;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class QaStatusCheckedEvent : IEvent, INotificationFilterByReleaseWindowId
    {
        public EventContext Context { get; set; }
        public IEnumerable<StatusCheckItem> StatusCheckItems { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[StatusCheckItems={0}, ReleaseWindowId={1}, Context={2}]",
                StatusCheckItems.FormatElements(), ReleaseWindowId, Context);
        }
    }
}
