using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseCalendar
{
    public class ReleaseWindowClosedEvent : IEvent
    {
        public EventContext Context { get; set; }

        public IEnumerable<Account> Recipients { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public bool IsFailed { get; set; }

        public override string ToString()
        {
            return String.Format("[Recipients={0}, ReleaseWindowId={1}, IsFailed={2}, Context={3}]",
                Recipients.FormatElements(), ReleaseWindowId, IsFailed, Context);
        }
    }
}
