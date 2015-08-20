using System;
using ReMi.BusinessEntities.HelpDesk;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class HelpDeskTaskCreatedEvent : IEvent, INotificationFilterByReleaseWindowId, INotificationFilterByReleaseTaskId
    {
        public HelpDeskTask HelpDeskTask { get; set; }

        public EventContext Context { get; set; }

        public Guid ReleaseWindowId { get { return HelpDeskTask.ReleaseWindowId; } }

        public Guid ReleaseTaskId { get { return HelpDeskTask.ReleaseTaskId; } }
    }
}
