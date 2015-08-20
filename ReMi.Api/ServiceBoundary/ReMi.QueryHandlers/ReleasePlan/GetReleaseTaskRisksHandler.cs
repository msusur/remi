using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetReleaseTaskRisksHandler : IHandleQuery<GetReleaseTaskRisksRequest, GetReleaseTaskRisksResponse>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGateway { get; set; }

        public GetReleaseTaskRisksResponse Handle(GetReleaseTaskRisksRequest request)
        {
            using (var gateway = ReleaseTaskGateway())
            {
                return new GetReleaseTaskRisksResponse
                {
                    Risks = gateway.GetReleaseTaskRisks()
                };
            }
        }
    }
}
