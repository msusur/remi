using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class CheckListQuestionRemovedEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public Guid ReleaseWindowGuid { get; set; }
        public Guid CheckListId { get; set; }

        public EventContext Context { get; set; }
        public Guid ReleaseWindowId
        {
            get { return ReleaseWindowGuid; }
        }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("ReleaseWindowGuid={0}, Question={1}, Context={2}",
                ReleaseWindowGuid, CheckListId, Context);
        }
    }
}
