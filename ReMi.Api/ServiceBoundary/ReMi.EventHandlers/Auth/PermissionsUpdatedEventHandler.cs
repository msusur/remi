using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Events.Auth;
using ReMi.Queries.Auth;

namespace ReMi.EventHandlers.Auth
{
    public class PermissionsUpdatedEventHandler : IHandleEvent<PermissionsUpdatedEvent>
    {
        public IHandleQuery<PermissionsRequest, PermissionsResponse> PermissionsQuery { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(PermissionsUpdatedEvent evnt)
        {
            var api = PermissionsQuery.Handle(new PermissionsRequest
            {
                RoleId = evnt.RoleId
            });

            EventPublisher.Publish(new PermissionsUpdatedUiEvent
            {
                RoleId = evnt.RoleId,
                Commands = api.Commands,
                Queries = api.Queries
            });
        }
    }
}
