using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data.DeploymentTool;

namespace ReMi.Contracts.Plugins.Services.DeploymentTool
{
    public interface IDeploymentTool : IPluginService
    {
        IEnumerable<ReleaseJob> GetReleaseJobs(IEnumerable<Guid> packageIds);

        IEnumerable<JobMetric> GetMetrics(IDictionary<Guid, IDictionary<Guid, int?>> packagesWithJobsAndLastBuildNumber);
    }
}
