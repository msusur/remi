using System;
using System.Linq;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryHandlers.ReleaseCalendar
{
    public class GetNearReleasesHandler : IHandleQuery<GetNearReleasesRequest, GetNearReleasesResponse>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }

        public GetNearReleasesResponse Handle(GetNearReleasesRequest request)
        {
            using (var releaseWindowGateway = ReleaseWindowGatewayFactory())
            {
                var result = releaseWindowGateway.GetNearReleases(request.Product).ToList();

                return new GetNearReleasesResponse
                {
                    ReleaseWindows = result
                };
            }
        }
    }
}
