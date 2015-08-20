using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetReleaseApproversQueryHandler : IHandleQuery<GetReleaseApproversRequest, GetReleaseApproversResponse>
    {
        public Func<IReleaseApproverGateway> ReleaseApproverGateway { get; set; }

        public GetReleaseApproversResponse Handle(GetReleaseApproversRequest request)
        {
            using (var gateway = ReleaseApproverGateway())
            {
                return new GetReleaseApproversResponse {Approvers = gateway.GetApprovers(request.ReleaseWindowId)};
            }
        }
    }
}
