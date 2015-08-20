using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class ReleaseParticipantsAddedEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }
        public IEnumerable<ReleaseParticipant> Participants { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, Participants={1}, Context={2}]",
                ReleaseWindowId, Participants.FormatElements(), Context);
        }
    }
}
