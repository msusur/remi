using System;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class CheckListAnswerUpdatedEvent : IEvent, INotificationFilterByReleaseWindowId
    {
        public Guid ReleaseWindowGuid { get; set; }
        public String AnsweredBy { get; set; }
        public Guid CheckListId { get; set; }
        public Boolean Checked { get; set; }

        public EventContext Context { get; set; }
        public Guid ReleaseWindowId
        {
            get { return ReleaseWindowGuid; }
        }

        public override string ToString()
        {
            return String.Format("ReleaseWindowGuid={0}, AnsweredBy={1}, CheckListId={2}, Checked={3}, Context={4}",
                ReleaseWindowGuid,
                AnsweredBy, CheckListId, Checked, Context);
        }
    }
}
