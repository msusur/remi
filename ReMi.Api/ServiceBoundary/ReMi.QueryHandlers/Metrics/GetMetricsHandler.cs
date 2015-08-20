using System;
using System.Linq;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.Queries.Metrics;

namespace ReMi.QueryHandlers.Metrics
{
    public class GetMetricsHandler : IHandleQuery<GetMetricsRequest, GetMetricsResponse>
    {
        public Func<IMetricsGateway> MetricsGatewayFactory { get; set; }

        public GetMetricsResponse Handle(GetMetricsRequest request)
        {
            using (var gateway = MetricsGatewayFactory())
            {
                return new GetMetricsResponse
                {
                    Metrics = gateway.GetMetrics(request.ReleaseWindowId).ToList()
                };
            }
        }
    }
}
