using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic.Metrics;
using ReMi.Common.Constants;
using ReMi.Common.Constants.Metrics;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.Exceptions.Configuration;
using ReMi.Queries.Metrics;

namespace ReMi.QueryHandlers.Metrics
{
    public class GetMeasurementsHandler : IHandleQuery<GetMeasurementsRequest, GetMeasurementsResponse>
    {
        public Func<IMetricsGateway> MetricsGatewayFactory { get; set; }
        public Func<IProductGateway> ProductGatewayFactory { get; set; }
        public IMeasurementsCalculator MeasurementsCalculator { get; set; }

        public GetMeasurementsResponse Handle(GetMeasurementsRequest request)
        {
            Product product;
            using (var gateway = ProductGatewayFactory())
            {
                product = gateway.GetProduct(request.Product);
            }
            if (product == null)
                throw new ProductNotFoundException(request.Product);

            IDictionary<ReleaseWindow, IEnumerable<Metric>> metrics;
            using (var gateway = MetricsGatewayFactory())
            {
                metrics = gateway.GetMetrics(request.Product);
            }

            if (product.ReleaseTrack == ReleaseTrack.Automated)
            {
                return new GetMeasurementsResponse
                {
                    Measurements = metrics.Where(x => x.Key.ReleaseType == ReleaseType.Automated)
                        .Select(x => MeasurementsCalculator.Calculate(x.Key, x.Value, MeasurementType.OverallTime, MeasurementType.DeployTime))
                        .Where(x => x != null && x.Metrics != null && x.Metrics.All(o => o != null))
                        .ToList()
                };
            }

            return new GetMeasurementsResponse
            {
                Measurements = metrics.
                    Where(x => x.Key.ReleaseType == ReleaseType.Scheduled)
                    .Select(
                        x =>
                            MeasurementsCalculator.Calculate(x.Key, x.Value, MeasurementType.OverallTime,
                                MeasurementType.DeployTime, MeasurementType.DownTime))
                    .Where(
                        x =>
                            x != null && x.Metrics != null &&
                            x.Metrics.Any(
                                m =>
                                    m != null &&
                                    m.Name == EnumDescriptionHelper.GetDescription(MeasurementType.OverallTime)) &&
                            x.Metrics.Any(
                                m =>
                                    m != null &&
                                    m.Name == EnumDescriptionHelper.GetDescription(MeasurementType.DeployTime)))
                    .ToList()
            };
        }
    }
}
