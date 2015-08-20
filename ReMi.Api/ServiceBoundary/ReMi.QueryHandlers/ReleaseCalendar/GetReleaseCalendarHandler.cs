using System;
using AutoMapper;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryHandlers.ReleaseCalendar
{
    public class GetReleaseCalendarHandler : IHandleQuery<GetReleaseCalendarRequest, GetReleaseCalendarResponse>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public ICommandDispatcher CommandDispatcher { get; set; }

        public GetReleaseCalendarResponse Handle(GetReleaseCalendarRequest request)
        {
            var filter = MappingEngine.Map<GetReleaseCalendarRequest, ReleaseCalendarFilter>(request);

            using (var releaseWindowGateway = ReleaseWindowGatewayFactory())
            {
                var results = releaseWindowGateway.GetAllStartingInTimeRange(
                    filter.StartDay.ToUniversalTime(),
                    filter.EndDay.AddDays(1).AddMilliseconds(-1).ToUniversalTime());

                return new GetReleaseCalendarResponse
                {
                    ReleaseWindows = results
                };
            }
        }
    }
}
