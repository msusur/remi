using System;
using ReMi.Commands.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Events.Auth;

namespace ReMi.CommandHandlers.Auth
{
    public class CreateRoleCommandHandler : IHandleCommand<CreateRoleCommand>
    {
        public Func<IRoleGateway> RoleGatewayFactory { get; set; }
        public IPublishEvent PublishEvent { get; set; }

        public void Handle(CreateRoleCommand command)
        {
            using (var gateway = RoleGatewayFactory())
            {
                gateway.CreateRole(command.Role);

                PublishEvent.Publish(new RoleCreatedEvent { Role = command.Role });
            }
        }
    }
}
