using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetReleaseTaskTypesHandler : IHandleQuery<GetReleaseTaskTypesRequest, GetReleaseTaskTypesResponse>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGatewayFactory { get; set; }

        public GetReleaseTaskTypesResponse Handle(GetReleaseTaskTypesRequest request)
        {
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                return new GetReleaseTaskTypesResponse
                {
                    ReleaseTaskTypes = gateway.GetReleaseTaskTypes()
                };
            }
        }
    }
}
