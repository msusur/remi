using System;
using ReMi.Commands.Configuration;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Events.Auth;

namespace ReMi.CommandHandlers.Configuration
{
    public class RemoveCommandFromRoleCommandHandler : IHandleCommand<RemoveCommandFromRoleCommand>
    {
        public Func<ICommandPermissionsGateway> CommandPermissionsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(RemoveCommandFromRoleCommand command)
        {
            using (var gateway = CommandPermissionsGatewayFactory())
            {
                gateway.RemoveCommandPermission(command.CommandId, command.RoleExternalId);
            }

            EventPublisher.Publish(new PermissionsUpdatedEvent
            {
                RoleId = command.RoleExternalId
            });
        }
    }
}
