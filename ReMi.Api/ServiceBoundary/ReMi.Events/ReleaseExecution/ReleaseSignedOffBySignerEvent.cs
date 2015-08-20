using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseExecution
{
    public class ReleaseSignedOffBySignerEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }

        public Guid AccountId { get; set; }

        public String Comment { get; set; }

        public Guid ReleaseWindowGuid { get; set; }

        public Guid ReleaseWindowId { get { return ReleaseWindowGuid; } }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("ReleaseWindowGuid={0}, AccountId={1}, Comment={2}, Context={3}",
                ReleaseWindowGuid, AccountId, Comment, Context);
        }
    }
}
