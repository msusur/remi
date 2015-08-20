using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleaseCalendar
{
    public class ReleaseWindowCanceledEvent : IEvent
    {
        public EventContext Context { get; set; }

        public ReleaseWindow ReleaseWindow { get; set; }

        public IEnumerable<Account> Participants { get; set; }

        public override string ToString()
        {
            return string.Format("[Context={0}, ReleaseWindow={1}, Participants={2}]",
                Context, ReleaseWindow, Participants.FormatElements());
        }
    }
}
