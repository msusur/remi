using AutoMapper;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Exceptions;
using ReMi.Commands.DeploymentTool;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Data.DeploymentTool;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;


namespace ReMi.CommandHandlers.DeploymentTool
{
    public class PopulateDeploymentMeasurementsCommandHandler : IHandleCommand<PopulateDeploymentMeasurementsCommand>
    {
        public Func<IReleaseJobGateway> ReleaseJobGatewayFactory { get; set; }
        public Func<IProductGateway> PackageGatewayFactory { get; set; }
        public Func<IReleaseDeploymentMeasurementGateway> ReleaseDeploymentMeasurementGatewayFactory { get; set; }
        public IDeploymentTool DeploymentToolService { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public void Handle(PopulateDeploymentMeasurementsCommand command)
        {
            IDictionary<Guid, int?> jobIds;
            using (var gateway = ReleaseJobGatewayFactory())
            {
                jobIds = gateway.GetReleaseJobs(command.ReleaseWindowId, true)
                    .Where(x => x.IsIncluded)
                    .ToDictionary(x => x.JobId, x => x.LastBuildNumber);
            }

            if (jobIds.IsNullOrEmpty())
                return;

            Guid packageId;
            using (var gateway = PackageGatewayFactory())
            {
                var packages = gateway.GetProducts(command.ReleaseWindowId);
                if (packages.Count() > 1)
                    throw new MoreThanOnePackageAssignToReleaseException(command.ReleaseWindowId);
                packageId = packages.First().ExternalId;
            }

            Logger.DebugFormat("Start getting data from deployment tool: {0}", DateTime.Now);

            var measurements = MappingEngine.Map<IEnumerable<JobMetric>, IEnumerable<JobMeasurement>>(
                    DeploymentToolService.GetMetrics(new Dictionary<Guid, IDictionary<Guid, int?>> { { packageId, jobIds } }))
                .ToArray();

            Logger.DebugFormat("Finish getting data from deployment tool: {0}", DateTime.Now);

            if (measurements.IsNullOrEmpty()) return;

            using (var gateway = ReleaseDeploymentMeasurementGatewayFactory())
            {
                gateway.StoreDeploymentMeasurements(measurements, command.ReleaseWindowId,
                    command.CommandContext.UserId);
            }

            Logger.DebugFormat("Data is stored in DB: {0}", DateTime.Now);
        }
    }


}
