using ReMi.Contracts.Plugins.Data.DeploymentTool;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Composites.Services
{
    public class DeploymentToolComposite : BaseComposit<IDeploymentTool>, IDeploymentTool
    {
        public IEnumerable<ReleaseJob> GetReleaseJobs(IEnumerable<Guid> packageIds)
        {
            return GetPackageServicesWithConfiguration(packageIds, PluginType.DeploymentTool)
                .Select(x => x.Service.GetReleaseJobs(x.Configurations.Select(c => c.PackageId)))
                .SelectMany(x => x)
                .ToArray();
        }

        public IEnumerable<JobMetric> GetMetrics(IDictionary<Guid, IDictionary<Guid, int?>> packagesWithJobsAndLastBuildNumber)
        {
            return packagesWithJobsAndLastBuildNumber
                .Select(x => GetPluginService(x.Key, PluginType.DeploymentTool)
                    .GetMetrics(new Dictionary<Guid, IDictionary<Guid, int?>> { { x.Key, x.Value } }))
                .SelectMany(x => x)
                .ToArray();
        }

        public bool AllowGettingDeployTime(Guid packageId)
        {
            var service = GetPluginService(packageId, PluginType.DeploymentTool);
            return service != null && service.AllowGettingDeployTime(packageId);
        }
    }
}
