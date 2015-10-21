using ReMi.Contracts.Plugins.Data.DeploymentTool;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Logging;
using ReMi.Plugin.Go.DataAccess.Gateways;
using ReMi.Plugin.Go.Entities;

namespace ReMi.Plugin.Go.BusinessLogic
{
    public class GoDeploymentTool : IDeploymentTool
    {
        public IGoRequest GoRequest { get; set; }
        public Func<IPackageConfigurationGateway> PackageConfigurationGatewayFactory { get; set; }
        public IMappingEngine Mapper { get; set; }

        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<ReleaseJob> GetReleaseJobs(IEnumerable<Guid> packageIds)
        {
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                var packageConfigurations = gateway.GetPackagesConfiguration(packageIds);
                return Mapper.Map<IEnumerable<GoPipelineConfiguration>, IEnumerable<ReleaseJob>>(
                    packageConfigurations.SelectMany(x => x.GoPipelines).ToArray());
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
                    if (configuration == null || string.IsNullOrEmpty(configuration.GoServerUrl))
                        continue;
                    var pipelineNames = gateway.GetPipelines(packagesWithJob.Value.Keys);

                    GoRequest.ChangeBaseUrl(configuration.GoServerUrl);
                    Parallel.ForEach(pipelineNames, p =>
                    {
                        var stepTiming = GoRequest.GetPipelineTiming(p.Name);
                        if (stepTiming == null) return;

                        var jobMetric = Mapper.Map<StepTiming, JobMetric>(stepTiming);
                        if (!result.TryAdd(p.Name, jobMetric))
                        {
                            Logger.ErrorFormat("Pipeline measurements not stored. Pipeline={0}, Id={1}",
                                p.Name, p.ExternalId);
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
                var packageConfigurations = gateway.GetPackageConfiguration(packageId);
                return packageConfigurations != null
                    && packageConfigurations.AllowGettingDeployTime == GettingDeployTimeMode.Allow;
            }
        }
    }
}
