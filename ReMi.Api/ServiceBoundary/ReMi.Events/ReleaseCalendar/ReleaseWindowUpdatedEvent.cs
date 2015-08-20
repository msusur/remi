using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseCalendar
{
    public class ReleaseWindowUpdatedEvent : IEvent
    {
        public override string ToString()
        {
            return string.Format("[Event = {0}, ReleaseWindow = {1}]",
                GetType().Name, ReleaseWindow);
        }

        public ReleaseWindow ReleaseWindow { get; set; }

        public EventContext Context { get; set; }
    }
}
