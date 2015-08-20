using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryHandlers.ReleaseCalendar
{
    public class GetCurrentReleaseHandler : IHandleQuery<GetCurrentReleaseRequest, GetCurrentReleaseResponse>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }

        public GetCurrentReleaseResponse Handle(GetCurrentReleaseRequest request)
        {
            using (var releaseWindowGateway = ReleaseWindowGatewayFactory())
            {
                var result = releaseWindowGateway.GetCurrentRelease(request.Product);

                return new GetCurrentReleaseResponse
                {
                    ReleaseWindow = result
                };
            }
        }
    }
}
