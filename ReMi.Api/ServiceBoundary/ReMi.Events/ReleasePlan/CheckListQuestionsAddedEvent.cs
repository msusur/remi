using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class CheckListQuestionsAddedEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public Guid ReleaseWindowGuid { get; set; }
        public IEnumerable<CheckListQuestion> Questions { get; set; }

        public EventContext Context { get; set; }
        public Guid ReleaseWindowId { get { return ReleaseWindowGuid; } }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("ReleaseWindowGuid={0}, Questions={1}, Context={2}", ReleaseWindowGuid,
                Questions.FormatElements(), Context);
        }
    }
}
