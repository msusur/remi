using Common.Logging;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Constants.Metrics;

namespace ReMi.BusinessLogic.Metrics
{
    public class MeasurementsCalculator : IMeasurementsCalculator
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public Measurement Calculate(ReleaseWindow releaseWindow, IEnumerable<Metric> metrics, params MeasurementType[] types)
        {
            if (types.IsNullOrEmpty() || releaseWindow == null)
                return null;

            return new Measurement
            {
                ReleaseWindow = releaseWindow,
                Metrics = types.Select(x => Calculate(x, releaseWindow, metrics))
            };
        }

        public MeasurementTimeByType Calculate(MeasurementType type, ReleaseWindow releaseWindow, IEnumerable<Metric> metrics)
        {
            if (releaseWindow == null)
                throw new ArgumentNullException("releaseWindow");

            switch (type)
            {
                case MeasurementType.OverallTime:
                    return CalculateOverallTime(releaseWindow);

                case MeasurementType.DownTime:
                    return CalculateTimeBetweenMetrics(releaseWindow, metrics, type, MetricType.SiteDown, MetricType.SiteUp);

                case MeasurementType.DeployTime:
                    return CalculateTimeBetweenMetrics(releaseWindow, metrics, type, MetricType.StartDeploy, MetricType.FinishDeploy);

                case MeasurementType.PreDownTime:
                    return CalculatePreDownTime(releaseWindow, metrics);

                case MeasurementType.PostDownTime:
                    return CalculatePostDownTime(releaseWindow, metrics);

                case MeasurementType.RunTime:
                    return CalculateTimeBetweenMetrics(releaseWindow, metrics, type, MetricType.StartRun, MetricType.FinishRun);
                default: return null;
            }
        }

        private static MeasurementTimeByType CalculateOverallTime(ReleaseWindow releaseWindow)
        {
            if (!releaseWindow.SignedOff.HasValue)
            {
                Log.WarnFormat("Release is not signed off. ReleaseId={0}", releaseWindow.ExternalId);
                return null;
            }

            var startTime = releaseWindow.StartTime;
            var signedOffTime = releaseWindow.SignedOff;

            return new MeasurementTimeByType
                {
                    Type = MeasurementType.OverallTime,
                    Value = ConvertToMinutes((signedOffTime.Value - startTime).TotalMinutes)
                };
        }

        private static MeasurementTimeByType CalculateTimeBetweenMetrics(ReleaseWindow releaseWindow, IEnumerable<Metric> metrics,
            MeasurementType measurementType, MetricType startMetricType, MetricType finishMetricType)
        {
            var metricDict = GetMetricTimes(new[] { startMetricType, finishMetricType }, metrics);
            if (metricDict == null)
            {
                Log.WarnFormat("Some metrics are absent. ReleaseId={0}, Metrics={1}", releaseWindow.ExternalId, new[] { startMetricType, finishMetricType }.FormatElements());
                return null;
            }

            var startMetricTime = metricDict[startMetricType];
            var finishMetricTime = metricDict[finishMetricType];

            if (finishMetricTime < startMetricTime)
                throw new ArgumentException(string.Format("{1} time less then {0} time", startMetricType, finishMetricType));

            return new MeasurementTimeByType
            {
                Type = measurementType,
                Value = ConvertToMinutes(
                    finishMetricTime.Subtract(startMetricTime).TotalMinutes
                )
            };
        }

        private static MeasurementTimeByType CalculatePreDownTime(ReleaseWindow releaseWindow, IEnumerable<Metric> metrics)
        {
            var siteDownTime = GetMetricTime(MetricType.SiteDown, metrics);
            if (!siteDownTime.HasValue)
            {
                Log.WarnFormat("Some metrics are absent. ReleaseId={0}, Metrics={1}", releaseWindow.ExternalId, new[] { MetricType.SiteDown }.FormatElements());
                return null;
            }

            if (siteDownTime < releaseWindow.StartTime)
                throw new ArgumentException("Site down time less then release start time");

            return new MeasurementTimeByType
                    {
                        Type = MeasurementType.PreDownTime,
                        Value = ConvertToMinutes((siteDownTime.Value - releaseWindow.StartTime).TotalMinutes)
                    };
        }

        private static MeasurementTimeByType CalculatePostDownTime(ReleaseWindow releaseWindow, IEnumerable<Metric> metrics)
        {
            var siteUpTime = GetMetricTime(MetricType.SiteUp, metrics);
            if (!siteUpTime.HasValue)
            {
                Log.WarnFormat("Some metrics are absent. ReleaseId={0}, Metrics={1}", releaseWindow.ExternalId, new[] { MetricType.SiteUp }.FormatElements());
                return null;
            }
            if (!releaseWindow.SignedOff.HasValue)
            {
                Log.WarnFormat("Release is not signed off. ReleaseId={0}", releaseWindow.ExternalId);
                return null;
            }

            if (releaseWindow.SignedOff.Value < siteUpTime.Value)
                throw new ArgumentException("release start down time less then release start time");

            return new MeasurementTimeByType
                {
                    Type = MeasurementType.PostDownTime,
                    Value = ConvertToMinutes((releaseWindow.SignedOff.Value - siteUpTime.Value).TotalMinutes)
                };
        }

        private static DateTime? GetMetricTime(MetricType type, IEnumerable<Metric> metrics)
        {
            var result = GetMetricTimes(new[] { type }, metrics);
            if (result == null)
                return null;

            return result[type];
        }

        private static IDictionary<MetricType, DateTime> GetMetricTimes(IEnumerable<MetricType> types, IEnumerable<Metric> metrics)
        {
            var enumerable = metrics as List<Metric> ?? metrics.ToList();
            if (enumerable.IsNullOrEmpty())
                throw new ArgumentNullException("metrics");

            var result = new Dictionary<MetricType, DateTime>();
            foreach (var type in types)
            {
                var metric = enumerable.FirstOrDefault(x => x.MetricType == type && x.ExecutedOn.HasValue);
                if (metric == null)
                    return null;

                result[type] = metric.ExecutedOn.Value;
            }

            return result;
        }

        private static int ConvertToMinutes(double value)
        {
            return value < 0 ? 0 : Convert.ToInt32(value);
        }
    }
}
