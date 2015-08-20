using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Metrics;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.Queries.ReleaseExecution;

namespace ReMi.QueryHandlers.ReleaseExecution
{
    public class GetDeploymentMeasurementsHandler : IHandleQuery<GetDeploymentMeasurementsRequest, GetDeploymentMeasurementsResponse>
    {
        public Func<IReleaseDeploymentMeasurementGateway> DeploymentMeasurementGatewayFactory { get; set; }
        public Func<IMetricsGateway> MetricsGatewayFactory { get; set; }

        public GetDeploymentMeasurementsResponse Handle(GetDeploymentMeasurementsRequest request)
        {
            using (var gateway = DeploymentMeasurementGatewayFactory())
            {
                var measurements = gateway.GetDeploymentMeasurements(request.ReleaseWindowId).ToArray();

                PopulateCompleteStatusByMetrics(measurements, request.ReleaseWindowId);

                return new GetDeploymentMeasurementsResponse
                {
                    Measurements = measurements
                };
            }
        }

        private void PopulateCompleteStatusByMetrics(IEnumerable<JobMeasurement> measurements, Guid releaseWindowId)
        {
            IEnumerable<Metric> metrics;
            using (var gateway = MetricsGatewayFactory())
            {
                metrics = gateway.GetMetrics(releaseWindowId)
                    .Where(o => o.MetricType == MetricType.SiteDown || o.MetricType == MetricType.SiteUp)
                    .ToList();
            }

            var siteDownMetric = metrics.FirstOrDefault(o => o.MetricType == MetricType.SiteDown);
            var siteDownTime = siteDownMetric != null ? siteDownMetric.ExecutedOn : null;
            var siteUpMetric = metrics.FirstOrDefault(o => o.MetricType == MetricType.SiteUp);
            var siteUpTime = siteUpMetric != null ? siteUpMetric.ExecutedOn : null;

            foreach (var measurement in measurements)
            {
                ProcessSteps(measurement, siteDownTime, siteUpTime);
            }
        }

        private void ProcessSteps(JobMeasurement measurement, DateTime? downTime, DateTime? upTime)
        {
            if (measurement == null)
                return;

            if (measurement.FinishTime.HasValue)
                measurement.JobStage = GetCompletedStage(measurement.FinishTime.Value, downTime, upTime);

            if (measurement.ChildSteps != null && measurement.ChildSteps.Any())
                foreach (var stepMeasurement in measurement.ChildSteps)
                {
                    ProcessSteps(stepMeasurement, downTime, upTime);
                }
        }

        private static JobStage GetCompletedStage(DateTime stageFinishTime, DateTime? siteDownTime, DateTime? siteUpTime)
        {
            if (siteDownTime.HasValue && stageFinishTime < siteDownTime.Value)
                return JobStage.BeforeOffline;

            if (siteDownTime.HasValue && siteUpTime.HasValue
                && stageFinishTime >= siteDownTime.Value && stageFinishTime <= siteUpTime.Value)
                return JobStage.DuringOffline;

            if (siteUpTime.HasValue && stageFinishTime > siteUpTime.Value)
                return JobStage.AfterOffline;

            return JobStage.AfterRelease;
        }
    }
}
