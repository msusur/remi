using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class ReleaseParticipationConfirmedEvent : IEvent, INotificationFilterByReleaseWindowId
    {
        public Guid ReleaseParticipantId { get; set; }
        public Guid ReleaseWindowGuid { get; set; }

        public EventContext Context { get; set; }
        public Guid ReleaseWindowId {
            get { return ReleaseWindowGuid; }
        }

        public override string ToString()
        {
            return String.Format("ReleaseWindowGuid={0}, ReleaseParticipantId={1}, Comtext={2}", ReleaseWindowGuid,
                ReleaseParticipantId, Context);
        }
    }
}
