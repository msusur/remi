using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetReleaseTaskEnvironmentsHandler : IHandleQuery<GetReleaseTaskEnvironmentsRequest, GetReleaseTaskEnvironmentsResponse>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGateway { get; set; }

        public GetReleaseTaskEnvironmentsResponse Handle(GetReleaseTaskEnvironmentsRequest request)
        {
            using (var gateway = ReleaseTaskGateway())
            {
                return new GetReleaseTaskEnvironmentsResponse
                {
                    Environments = gateway.GetReleaseTaskEnvironments()
                };
            }
        }
    }
}
