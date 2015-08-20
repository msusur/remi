using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseExecution;

namespace ReMi.DataAccess.BusinessEntityGateways.Metrics
{
    public interface IMetricsGateway : IDisposable
    {
        IEnumerable<Metric> GetMetrics(Guid releaseWindowId, bool withoutBackground = true);
        IDictionary<ReleaseWindow, IEnumerable<Metric>> GetMetrics(string productName);

        Metric GetMetric(Guid releaseWindowId, MetricType metricType);

        void CreateOrUpdateMetric(Guid releaseWindowId, Metric metric);
        void CreateOrUpdateMetric(Guid releaseWindowId, MetricType metricType, DateTime? executedOn = null, Guid? externalId = null);

        void UpdateMetrics(Metric metrics);
        void CreateMetrics(Guid releaseWindowId, IEnumerable<MetricType> metricTypes);
        void DeleteMetrics(Guid releaseWindowId);

        void CreateMetrics(Guid releaseWindowId, IDictionary<MetricType, DateTime?> metrics);
    }
}
