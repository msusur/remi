using ReMi.Contracts.Cqrs.Events;
using System;

namespace ReMi.Events.Auth
{
    public class PermissionsUpdatedEvent : IEvent
    {
        public EventContext Context { get; set; }
        public Guid RoleId { get; set; }

        public override string ToString()
        {
            return String.Format("[RoleId={0},Context={1}]", RoleId, Context);
        }
    }
}
