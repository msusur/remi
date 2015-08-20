using System;
using ReMi.Commands.Configuration;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Events.Auth;

namespace ReMi.CommandHandlers.Configuration
{
    public class RemoveQueryFromRoleCommandHandler : IHandleCommand<RemoveQueryFromRoleCommand>
    {
        public Func<IQueryPermissionsGateway> QueryPermissionsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(RemoveQueryFromRoleCommand command)
        {
            using (var gateway = QueryPermissionsGatewayFactory())
            {
                gateway.RemoveQueryPermission(command.QueryId, command.RoleExternalId);
            }

            EventPublisher.Publish(new PermissionsUpdatedEvent
            {
                RoleId = command.RoleExternalId
            });
        }
    }
}
