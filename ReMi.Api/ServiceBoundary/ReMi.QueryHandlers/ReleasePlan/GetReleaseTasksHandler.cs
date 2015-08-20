using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetReleaseTasksHandler : IHandleQuery<GetReleaseTasksRequest, GetReleaseTasksResponse>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGatewayFactory { get; set; }

        public GetReleaseTasksResponse Handle(GetReleaseTasksRequest request)
        {
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                return new GetReleaseTasksResponse
                {
                    ReleaseTasks = gateway.GetReleaseTaskViews(request.ReleaseWindowId)
                };
            }
        }
    }
}
