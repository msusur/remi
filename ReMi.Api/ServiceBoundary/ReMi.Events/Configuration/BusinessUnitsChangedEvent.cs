using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.Configuration
{
    public class BusinessUnitsChangedEvent : IEvent, INotificationFilterByNotRequestor
    {
        public EventContext Context { get; set; }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("[Context={0}]", Context);
        }
    }
}
