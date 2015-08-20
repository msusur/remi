using ReMi.Common.Constants;
using ReMi.Common.WebApi.Notifications;
using System;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events
{
    public class NotificationOccuredForUserEvent : IEvent, INotificationFilterForUser
    {
        public NotificationOccuredForUserEvent()
        {
            Type = NotificationOccuredEventType.Info;
        }

        public string Code { get; set; }

        public string Message { get; set; }

        public NotificationOccuredEventType Type { get; set; }

        public EventContext Context { get; set; }

        public Guid AccountId { get { return Context.UserId; } }

        public override string ToString()
        {
            return string.Format("[Code={0}, Message={1}, Context={2}]",
                Code, Message, Context);
        }
    }
}
