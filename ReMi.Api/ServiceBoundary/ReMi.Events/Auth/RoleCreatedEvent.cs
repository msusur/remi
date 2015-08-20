using ReMi.BusinessEntities.Auth;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.Auth
{
    public class RoleCreatedEvent : IEvent, INotificationFilter
    {
        public EventContext Context { get; set; }

        public Role Role { get; set; }
    }
}
