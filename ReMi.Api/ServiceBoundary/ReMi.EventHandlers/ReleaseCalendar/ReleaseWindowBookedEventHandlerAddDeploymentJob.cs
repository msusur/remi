using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Common.Logging;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.ReleaseCalendar
{
    public class ReleaseWindowBookedEventHandlerAddDeploymentJob : IHandleEvent<ReleaseWindowBookedEvent>
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public Func<IReleaseJobGateway> ReleaseJobGatewayFactory { get; set; }
        public Func<IProductGateway> PackageGatewayFactory { get; set; }
        public IDeploymentTool DeploymentToolService { get; set; }
        public IReleaseWindowHelper ReleaseWindowHelper { get; set; }
        public IMappingEngine Mapper { get; set; }

        public void Handle(ReleaseWindowBookedEvent evnt)
        {
            if (ReleaseWindowHelper.IsMaintenance(evnt.ReleaseWindow))
            {
                Logger.DebugFormat("Cancel adding jobs to maintenance window. Type={0}, ExternalId={1}",
                    evnt.ReleaseWindow.ReleaseType, evnt.ReleaseWindow.ExternalId);
                return;
            }

            IEnumerable<Guid> packageIds;
            using (var gateway = PackageGatewayFactory())
                packageIds = gateway.GetProducts(evnt.ReleaseWindow.ExternalId).Select(x => x.ExternalId).ToArray();

            var deploymentJobs = Mapper.Map<IEnumerable<Contracts.Plugins.Data.DeploymentTool.ReleaseJob>, IEnumerable<ReleaseJob>>(
                    DeploymentToolService.GetReleaseJobs(packageIds)
                    .Where(x => !x.IsDisabled))
                .ToArray();

            if (deploymentJobs.IsNullOrEmpty()) return;

            using (var gateway = ReleaseJobGatewayFactory())
            {
                var existingJobs = gateway.GetReleaseJobs(evnt.ReleaseWindow.ExternalId).ToArray();

                foreach (var job in deploymentJobs
                    .Where(job => job.IsIncluded && existingJobs.All(o => o.JobId != job.JobId)))
                {
                    gateway.AddJobToRelease(job, evnt.ReleaseWindow.ExternalId);
                }
            }
        }
    }
}
