using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.Auth;

namespace ReMi.QueryHandlers.Auth
{
    public class PermissionsHandler : IHandleQuery<PermissionsRequest, PermissionsResponse>
    {
        public Func<ICommandPermissionsGateway> CommandPermissionsGatewayFactory { get; set; }
        public Func<IQueryPermissionsGateway> QueryPermissionsGatewayFactory { get; set; }

        public PermissionsResponse Handle(PermissionsRequest request)
        {
            var response = new PermissionsResponse();

            using (var commandPermissionsGateway = CommandPermissionsGatewayFactory())
            {
                response.Commands = commandPermissionsGateway.GetAllowedCommands(request.RoleId);
            }

            using (var queryPermissionsGateway = QueryPermissionsGatewayFactory())
            {
                response.Queries = queryPermissionsGateway.GetAllowedQueries(request.RoleId);
            }

            return response;
        }
    }
}
