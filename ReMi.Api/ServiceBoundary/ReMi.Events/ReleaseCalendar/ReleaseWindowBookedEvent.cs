using System.Collections.Generic;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseCalendar
{
    public class ReleaseWindowBookedEvent : IEvent, INotificationFilterByProduct
    {
        public ReleaseWindow ReleaseWindow { get; set; }

        public EventContext Context { get; set; }

        public IEnumerable<string> Products
        {
            get { return ReleaseWindow.Products; }
        }

        public override string ToString()
        {
            return string.Format("[Event={0}, ReleaseWindow={1}]", GetType().Name, ReleaseWindow);
        }
    }
}
