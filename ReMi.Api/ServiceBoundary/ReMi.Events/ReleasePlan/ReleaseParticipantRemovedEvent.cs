using System;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class ReleaseParticipantRemovedEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public ReleaseParticipant Participant { get; set; }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, Participant={1}, Context={2}]",
                ReleaseWindowId, Participant, Context);
        }
    }
}
