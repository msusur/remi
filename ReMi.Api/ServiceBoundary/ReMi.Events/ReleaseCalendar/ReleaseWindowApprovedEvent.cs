using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseCalendar
{
    public class ReleaseWindowApprovedEvent : IEvent
    {
        public EventContext Context { get; set; }

        public ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return string.Format("[Event={0}, ReleaseWindow={1}, Context={2}]", GetType().Name, ReleaseWindow, Context);
        }
    }
}
