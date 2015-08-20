using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseExecution
{
    public class ReleaseSignersAddedEvent : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
    {
        public EventContext Context { get; set; }
        public List<SignOff> SignOffs { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public Guid RequestorId { get { return Context.UserId; } }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, SignOffs={1}, Context={2}]",
                ReleaseWindowId, SignOffs.FormatElements(), Context);
        }
    }
}
