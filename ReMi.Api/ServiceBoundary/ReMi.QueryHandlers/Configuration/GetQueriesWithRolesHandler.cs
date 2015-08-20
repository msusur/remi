using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.Configuration;

namespace ReMi.QueryHandlers.Configuration
{
    public class GetQueriesWithRolesHandler : IHandleQuery<GetQueriesWithRolesRequest, GetQueriesWithRolesResponse>
    {
        public Func<IQueryPermissionsGateway> QueryPermissionsGatewayFactory { get; set; }

        public GetQueriesWithRolesResponse Handle(GetQueriesWithRolesRequest request)
        {
            using (var gateway = QueryPermissionsGatewayFactory())
            {
                return new GetQueriesWithRolesResponse
                {
                    Queries = gateway.GetQueries()
                };
            }
        }
    }
}
