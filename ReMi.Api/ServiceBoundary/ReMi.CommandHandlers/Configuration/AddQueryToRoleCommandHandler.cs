using System;
using ReMi.Commands.Configuration;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Events.Auth;

namespace ReMi.CommandHandlers.Configuration
{
    public class AddQueryToRoleCommandHandler : IHandleCommand<AddQueryToRoleCommand>
    {
        public Func<IQueryPermissionsGateway> QueryPermissionsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(AddQueryToRoleCommand command)
        {
            using (var gateway = QueryPermissionsGatewayFactory())
            {
                gateway.AddQueryPermission(command.QueryId, command.RoleExternalId);
            }

            EventPublisher.Publish(new PermissionsUpdatedEvent
            {
                RoleId = command.RoleExternalId
            });
        }
    }
}
