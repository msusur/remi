using ReMi.Contracts.Plugins.Data.DeploymentTool;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Logging;
using ReMi.Plugin.Jenkins.DataAccess.Gateways;
using ReMi.Plugin.Jenkins.JenkinsApi;

namespace ReMi.Plugin.Jenkins.BusinessLogic
{
    public class JenkinsDeploymentTool : IDeploymentTool
    {
        public IJenkinsRequest JenkinsRequest { get; set; }
        public Func<IPackageConfigurationGateway> PackageConfigurationGatewayFactory { get; set; }
        public IMappingEngine Mapper { get; set; }

        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<ReleaseJob> GetReleaseJobs(IEnumerable<Guid> packageIds)
        {
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                var packageConfigurations = gateway.GetPackagesConfiguration(packageIds);
                return Mapper.Map<IEnumerable<JenkinsJobConfiguration>, IEnumerable<ReleaseJob>>(
                    packageConfigurations.SelectMany(x => x.JenkinsJobs).ToArray());
            }
        }

        public IEnumerable<JobMetric> GetMetrics(IDictionary<Guid, IDictionary<Guid, int?>> packagesWithJobsAndLastBuildNumber)
        {
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                var result = new ConcurrentDictionary<string, JobMetric>();
                foreach (var packagesWithJob in packagesWithJobsAndLastBuildNumber)
                {
                    var configuration = gateway.GetPackageConfiguration(packagesWithJob.Key);
                    if (configuration == null || string.IsNullOrEmpty(configuration.JenkinsServerUrl))
                        continue;
                    var jobs = gateway.GetJobs(packagesWithJob.Value.Keys)
                        .Join(packagesWithJob.Value, j => j.ExternalId, k => k.Key,
                            (j, k) => new {Job = j, BuildNumber = k.Value});

                    JenkinsRequest.ChangeBaseUrl(configuration.JenkinsServerUrl);
                    JenkinsRequest.ChangeBaseUrl(configuration.JenkinsServerUrl);
                    Parallel.ForEach(jobs, j =>
                    {
                        var metrics = JenkinsRequest.GetJobMetrics(j.Job.Name, j.BuildNumber, configuration.TimeZone);
                        if (metrics == null) return;

                        if (!result.TryAdd(j.Job.Name, metrics))
                        {
                            Logger.ErrorFormat("Jenkins Job measurements not stored. Job={0}, Id={1}",
                                j.Job.Name, j.Job.ExternalId);
                        }
                    });
                }
                return result.Values.ToArray();
            }
        }

        public bool AllowGettingDeployTime(Guid packageId)
        {
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                var packageConfiguration = gateway.GetPackageConfiguration(packageId);
                return packageConfiguration != null
                    && packageConfiguration.AllowGettingDeployTime == GettingDeployTimeMode.Allow;
            }
        }
    }
}
