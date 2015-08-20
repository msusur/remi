using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetReleaseTaskHandler : IHandleQuery<GetReleaseTaskRequest, GetReleaseTaskResponse>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGateway { get; set; }

        public GetReleaseTaskResponse Handle(GetReleaseTaskRequest request)
        {
            using (var gateway = ReleaseTaskGateway())
            {
                var task = gateway.GetReleaseTask(request.ReleaseTaskId);

                return new GetReleaseTaskResponse { ReleaseTask = task };
            }
        }
    }
}
