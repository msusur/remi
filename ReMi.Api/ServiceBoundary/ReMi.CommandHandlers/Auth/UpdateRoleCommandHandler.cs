using System;
using ReMi.Commands.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Events.Auth;

namespace ReMi.CommandHandlers.Auth
{
    public class UpdateRoleCommandHandler : IHandleCommand<UpdateRoleCommand>
    {
        public Func<IRoleGateway> RoleGatewayFactory { get; set; }
        public IPublishEvent PublishEvent { get; set; }

        public void Handle(UpdateRoleCommand command)
        {
            using (var gateway = RoleGatewayFactory())
            {
                gateway.UpdateRole(command.Role);

                PublishEvent.Publish(new RoleUpdatedEvent { Role = command.Role });
            }
        }
    }
}
