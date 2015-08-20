using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryHandlers.ReleaseCalendar
{
    public class GetUpcomingReleaseHandler : IHandleQuery<GetUpcomingReleaseRequest, GetUpcomingReleaseResponse>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }

        public GetUpcomingReleaseResponse Handle(GetUpcomingReleaseRequest request)
        {
            using (var releaseWindowGateway = ReleaseWindowGatewayFactory())
            {
                var result = releaseWindowGateway.GetUpcomingRelease(request.Product);

                return new GetUpcomingReleaseResponse
                {
                    ReleaseWindow = result
                };
            }
        }
    }
}
