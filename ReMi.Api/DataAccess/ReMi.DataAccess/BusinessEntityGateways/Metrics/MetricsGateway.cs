using AutoMapper;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Metrics;
using ReMi.DataEntities.ReleaseCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using DataMetric = ReMi.DataEntities.Metrics.Metric;
using Metric = ReMi.BusinessEntities.Metrics.Metric;

namespace ReMi.DataAccess.BusinessEntityGateways.Metrics
{
    public class MetricsGateway : BaseGateway, IMetricsGateway
    {
        public IRepository<DataMetric> MetricsRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IMappingEngine Mapper { get; set; }
        public IApplicationSettings ApplicationSettings { get; set; }

        public IEnumerable<Metric> GetMetrics(Guid releaseWindowId, bool withoutBackground = true)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);
            if (releaseWindow == null)
            {
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            }

            if (releaseWindow.Metrics == null)
            {
                return Enumerable.Empty<Metric>();
            }

            var metrics = (IEnumerable<DataMetric>)releaseWindow.Metrics.ToArray();
            if (withoutBackground)
            {
                metrics = metrics.Where(x =>
                {
                    var desc = x.MetricType.ToEnumDescription<MetricType, MetricTypeDescription>();
                    return !desc.IsBackground.HasValue || !desc.IsBackground.Value;
                });
            }
            return Mapper.Map<IEnumerable<DataMetric>, IEnumerable<Metric>>(metrics);
        }

        public IDictionary<BusinessEntities.ReleaseCalendar.ReleaseWindow, IEnumerable<Metric>> GetMetrics(string productName)
        {
            var items = ReleaseWindowRepository
                .GetAllSatisfiedBy(x => x.ReleaseProducts.Any(p => p.Product.Description == productName)
                                    && x.Metrics.Any()
                                    && x.Metrics.Any(y => y.MetricType == MetricType.SignOff && y.ExecutedOn.HasValue)).ToList();

            return items.ToDictionary(
                x => Mapper.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(x),
                x => Mapper.Map<IEnumerable<DataMetric>, IEnumerable<Metric>>(x.Metrics)
            );
        }

        public Metric GetMetric(Guid releaseWindowId, MetricType metricType)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);
            if (releaseWindow == null)
            {
                throw new ReleaseWindowNotFoundException(releaseWindowId);
            }

            if (!releaseWindow.Metrics.Any())
            {
                return null;
            }

            var metric = releaseWindow.Metrics.FirstOrDefault(x => x.MetricType == metricType);

            return metric == null ? null : Mapper.Map<DataMetric, Metric>(metric);
        }

        public void CreateOrUpdateMetric(Guid releaseWindowId, Metric metric)
        {
            CreateOrUpdateMetric(releaseWindowId, metric.MetricType, metric.ExecutedOn, metric.ExternalId);
        }

        public void CreateOrUpdateMetric(Guid releaseWindowId, MetricType metricType, DateTime? executedOn = null,
            Guid? externalId = null)
        {
            DataMetric existingMetric = null;
            var release = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);
            if (release == null)
                throw new EntityNotFoundException(typeof(ReleaseWindow), releaseWindowId);

            if (externalId != null && !Guid.Empty.Equals(externalId))
            {
                existingMetric = MetricsRepository.GetSatisfiedBy(o => o.ExternalId == externalId);
            }
            if (existingMetric == null)
            {
                existingMetric = MetricsRepository.GetSatisfiedBy(x =>
                    x.ReleaseWindow.ExternalId == releaseWindowId && x.MetricType == metricType);
            }

            if (existingMetric != null)
            {
                existingMetric.ExecutedOn = executedOn.HasValue ? executedOn.Value.ToUniversalTime() : (DateTime?)null;
                MetricsRepository.Update(existingMetric);

                return;
            }

            CreateMetric(release.ReleaseWindowId, metricType, executedOn, externalId);
        }

        public void UpdateMetrics(Metric metrics)
        {
            var metric = MetricsRepository.GetSatisfiedBy(x => x.ExternalId == metrics.ExternalId);

            metric.ExecutedOn = metrics.ExecutedOn.HasValue ? metrics.ExecutedOn.Value.ToUniversalTime() : (DateTime?)null;

            MetricsRepository.Update(metric);
        }

        public void CreateMetrics(Guid releaseWindowId, IEnumerable<MetricType> metricTypes)
        {
            var release = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);

            CreateMetrics(releaseWindowId, metricTypes.ToDictionary(x => x, x => GetDefaultExecutedOn(x, release)));
        }

        public void CreateMetrics(Guid releaseWindowId, IDictionary<MetricType, DateTime?> metrics)
        {
            var release = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);

            foreach (var metric in metrics)
            {
                if (release.Metrics.All(x => x.MetricType != metric.Key))
                    CreateMetric(release.ReleaseWindowId, metric.Key, metric.Value);
            }
        }

        public void DeleteMetrics(Guid releaseWindowId)
        {
            var releaseId = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId).ReleaseWindowId;

            var metrics = MetricsRepository.GetAllSatisfiedBy(x => x.ReleaseWindowId == releaseId);

            metrics.ToList().ForEach(m => MetricsRepository.Delete(m));
        }

        #region Helpers

        private void CreateMetric(int releaseWindowId, MetricType metricType, DateTime? executedOn = null, Guid? externalId = null)
        {
            var metric = new DataMetric
            {
                MetricType = metricType,
                ExternalId = externalId != null && !Guid.Empty.Equals(externalId) ? externalId.Value : Guid.NewGuid(),
                ReleaseWindowId = releaseWindowId,
                ExecutedOn = executedOn.HasValue ? executedOn.Value.ToUniversalTime() : (DateTime?)null
            };

            MetricsRepository.Insert(metric);
        }

        private DateTime? GetDefaultExecutedOn(MetricType metricType, ReleaseWindow release)
        {
            switch (metricType)
            {
                case MetricType.StartTime:
                    return release.StartTime;

                case MetricType.EndTime:
                    var defaultReleaseWindowDurationTime = ApplicationSettings.DefaultReleaseWindowDurationTime;

                    return release.StartTime.AddMinutes(defaultReleaseWindowDurationTime);
            }

            return null;
        }

        #endregion
    }
}
