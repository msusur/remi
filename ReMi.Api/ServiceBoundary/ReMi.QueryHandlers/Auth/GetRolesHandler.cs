using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.Auth;

namespace ReMi.QueryHandlers.Auth
{
    public class GetRolesHandler : IHandleQuery<GetRolesRequest, GetRolesResponse>
    {
        public Func<IRoleGateway> RoleGatewayFactory { get; set; }

        public GetRolesResponse Handle(GetRolesRequest request)
        {
            using (var gateway = RoleGatewayFactory())
            {
                return new GetRolesResponse
                {
                    Roles = gateway.GetRoles()
                };
            }
        }
    }
}
