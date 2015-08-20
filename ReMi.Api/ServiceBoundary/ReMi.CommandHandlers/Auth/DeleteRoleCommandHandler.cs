using System;
using ReMi.Commands.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Events.Auth;

namespace ReMi.CommandHandlers.Auth
{
    public class DeleteRoleCommandHandler : IHandleCommand<DeleteRoleCommand>
    {
        public Func<IRoleGateway> RoleGatewayFactory { get; set; }
        public IPublishEvent PublishEvent { get; set; }

        public void Handle(DeleteRoleCommand command)
        {
            using (var gateway = RoleGatewayFactory())
            {
                gateway.DeleteRole(command.Role);

                PublishEvent.Publish(new RoleDeletedEvent { Role = command.Role });
            }
        }
    }
}
