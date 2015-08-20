using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.Configuration;

namespace ReMi.QueryHandlers.Configuration
{
    public class GetCommandsWithRolesHandler : IHandleQuery<GetCommandsWithRolesRequest, GetCommandsWithRolesResponse>
    {
        public Func<ICommandPermissionsGateway> CommandPermissionsGatewayFactory { get; set; }

        public GetCommandsWithRolesResponse Handle(GetCommandsWithRolesRequest request)
        {
            using (var gateway = CommandPermissionsGatewayFactory())
            {
                return new GetCommandsWithRolesResponse
                {
                    Commands = gateway.GetCommands()
                };
            }
        }
    }
}
