using System;
using ReMi.Commands.Configuration;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Events.Auth;

namespace ReMi.CommandHandlers.Configuration
{
    public class AddCommandToRoleCommandHandler : IHandleCommand<AddCommandToRoleCommand>
    {
        public Func<ICommandPermissionsGateway> CommandPermissionsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(AddCommandToRoleCommand command)
        {
            using (var gateway = CommandPermissionsGatewayFactory())
            {
                gateway.AddCommandPermission(command.CommandId, command.RoleExternalId);
            }

            EventPublisher.Publish(new PermissionsUpdatedEvent
            {
                RoleId = command.RoleExternalId
            });
        }
    }
}
