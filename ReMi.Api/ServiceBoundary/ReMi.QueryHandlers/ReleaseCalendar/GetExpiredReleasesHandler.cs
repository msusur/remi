using System;
using System.Linq;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryHandlers.ReleaseCalendar
{
    public class GetExpiredReleasesHandler : IHandleQuery<GetExpiredReleasesRequest, GetExpiredReleasesResponse>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }

        public GetExpiredReleasesResponse Handle(GetExpiredReleasesRequest request)
        {
            using (var releaseWindowGateway = ReleaseWindowGatewayFactory())
            {
                var result = releaseWindowGateway.GetExpiredReleases().ToList();

                return new GetExpiredReleasesResponse
                {
                    ReleaseWindows = result
                };
            }
        }
    }
}
