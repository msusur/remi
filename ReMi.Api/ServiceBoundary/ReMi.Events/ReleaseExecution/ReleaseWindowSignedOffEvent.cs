using System;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseExecution
{
    public class ReleaseWindowSignedOffEvent : IEvent
    {
        public EventContext Context { get; set; }
        public ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindow={0}, Context={1}]", ReleaseWindow, Context);
        }
    }
}
