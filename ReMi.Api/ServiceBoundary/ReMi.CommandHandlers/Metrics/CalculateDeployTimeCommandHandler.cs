using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Commands.Metrics;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.ReleaseExecution;

namespace ReMi.CommandHandlers.Metrics
{
    public class CalculateDeployTimeCommandHandler : IHandleCommand<CalculateDeployTimeCommand>
    {
        public IDeploymentTool DeploymentTool { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }
        public Func<IProductGateway> PackageGatewayFactory { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public IHandleQuery<GetDeploymentMeasurementsRequest, GetDeploymentMeasurementsResponse> GetDeploymentMeasurementsAction { get; set; }

        public void Handle(CalculateDeployTimeCommand command)
        {
            var package = GetPackage(command.ReleaseWindowId);
            if (!DeploymentTool.AllowGettingDeployTime(package.ExternalId))
                return;

            var releaseWindow = GetReleaseWindow(command.ReleaseWindowId);
            var jobMeasurements = GetJobMeasurements(command);

            DateTime? startDeploy = null, finishDeploy = null;
            foreach (var jobMeasurement in jobMeasurements
                .Where(x => !releaseWindow.RequiresDowntime || x.JobStage == JobStage.DuringOffline))
            {
                if (jobMeasurement.StartTime.HasValue
                    && (!startDeploy.HasValue || jobMeasurement.StartTime < startDeploy))
                {
                    startDeploy = jobMeasurement.StartTime;
                }
                if (jobMeasurement.FinishTime.HasValue
                    && (!finishDeploy.HasValue || jobMeasurement.FinishTime > finishDeploy))
                {
                    finishDeploy = jobMeasurement.FinishTime;
                }
            }

            if (!startDeploy.HasValue || !finishDeploy.HasValue) return;
            
            CommandDispatcher.Send(new UpdateMetricsWithDateTimeCommand
            {
                ReleaseWindowId = command.ReleaseWindowId,
                MetricType = MetricType.StartDeploy,
                ExecutedOn = startDeploy.Value,
                CommandContext = command.CommandContext.CreateChild()
            });
            CommandDispatcher.Send(new UpdateMetricsWithDateTimeCommand
            {
                ReleaseWindowId = command.ReleaseWindowId,
                MetricType = MetricType.FinishDeploy,
                ExecutedOn = finishDeploy.Value,
                CommandContext = command.CommandContext.CreateChild()
            });
        }

        private IEnumerable<JobMeasurement> GetJobMeasurements(CalculateDeployTimeCommand command)
        {
            var response = GetDeploymentMeasurementsAction.Handle(new GetDeploymentMeasurementsRequest
            {
                ReleaseWindowId = command.ReleaseWindowId,
                Context = command.CommandContext.CreateChild<QueryContext>()
            });

            return response != null && response.Measurements != null
                ? response.Measurements
                : new JobMeasurement[0];
        }

        private ReleaseWindow GetReleaseWindow(Guid releaseWindowId)
        {
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                return gateway.GetByExternalId(releaseWindowId);
            }
        }

        private Product GetPackage(Guid releaseWindowId)
        {
            using (var gateway = PackageGatewayFactory())
            {
                return gateway.GetProducts(releaseWindowId).First();
            }
        }
    }
}
