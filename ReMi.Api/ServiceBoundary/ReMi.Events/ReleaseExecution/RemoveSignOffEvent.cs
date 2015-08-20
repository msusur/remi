using ReMi.Common.WebApi.Notifications;
using System;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseExecution
{
    public class RemoveSignOffEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }

        public Guid SignOffId { get; set; }

        public Guid AccountId { get; set; }

        public Guid ReleaseWindowGuid { get; set; }

        public Guid ReleaseWindowId
        {
            get { return ReleaseWindowGuid; }
        }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("ReleaseWindowGuid={0}, SignOffId={1}, AccountId={2}, Context={3}", 
                ReleaseWindowGuid, SignOffId, AccountId, Context);
        }
    }
}
