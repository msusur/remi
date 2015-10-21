using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Queries.Metrics;
using System;
using System.Linq;

namespace ReMi.QueryHandlers.Metrics
{
    public class GetMetricsHandler : IHandleQuery<GetMetricsRequest, GetMetricsResponse>
    {
        public Func<IMetricsGateway> MetricsGatewayFactory { get; set; }
        public Func<IProductGateway> ProductGatewayFactory { get; set; }
        public IDeploymentTool DeploymentToolService { get; set; }

        public GetMetricsResponse Handle(GetMetricsRequest request)
        {
            Guid packageId;
            using (var gateway = ProductGatewayFactory())
            {
                var packages = gateway.GetProducts(request.ReleaseWindowId);
                packageId = packages.IsNullOrEmpty() ? Guid.Empty : packages.First().ExternalId;
            }
            var allowMeasureDeploymentTime = DeploymentToolService.AllowGettingDeployTime(packageId);
            using (var gateway = MetricsGatewayFactory())
            {
                return new GetMetricsResponse
                {
                    AutomaticDeployTime = allowMeasureDeploymentTime,
                    Metrics = gateway.GetMetrics(request.ReleaseWindowId)
                        .Where(x => !allowMeasureDeploymentTime
                            || (x.MetricType != MetricType.StartDeploy && x.MetricType != MetricType.FinishDeploy))
                        .ToList()
                };
            }
        }
    }
}
