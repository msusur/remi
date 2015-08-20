using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Contracts.Plugins.Services.SourceControl;

namespace ReMi.Plugin.Composites.Services
{
    public class SourceControlComposite : BaseComposit<ISourceControl>, ISourceControl
    {
        public IEnumerable<ReleaseRepository> GetRepositories(IEnumerable<Guid> packageIds)
        {
            return GetPackageServicesWithConfiguration(packageIds, PluginType.SourceControl)
                .Select(x => x.Service.GetRepositories(x.Configurations.Select(c => c.PackageId)))
                .SelectMany(x => x)
                .ToArray();
        }

        public IEnumerable<SourceControlChange> GetChanges(IEnumerable<Guid> packageIds)
        {
            return GetPackageServicesWithConfiguration(packageIds, PluginType.SourceControl)
                .Select(x => x.Service.GetChanges(x.Configurations.Select(c => c.PackageId)))
                .SelectMany(x => x)
                .ToArray();
        }

        public IEnumerable<SourceControlChange> GetChangesByReleaseJobs(IEnumerable<Guid> packageIds, IEnumerable<Guid> jobIds)
        {
            return GetPackageServicesWithConfiguration(packageIds, PluginType.SourceControl)
                .Select(x => x.Service.GetChangesByReleaseJobs(x.Configurations.Select(c => c.PackageId), jobIds))
                .SelectMany(x => x)
                .ToArray();
        }

        public IEnumerable<SourceControlChange> GetChangesByRepository(IEnumerable<Guid> packageIds, IEnumerable<ReleaseRepository> repositories)
        {
            return GetPackageServicesWithConfiguration(packageIds, PluginType.SourceControl)
                .Select(x => x.Service.GetChangesByRepository(x.Configurations.Select(c => c.PackageId), repositories))
                .SelectMany(x => x)
                .ToArray();
        }

        public IDictionary<Guid, SourceControlRetrieveMode> GetSourceControlRetrieveMode(IEnumerable<Guid> packageIds)
        {
            return packageIds.IsNullOrEmpty() ? null
                : packageIds.ToDictionary(x => x, x =>
                {
                    var service = GetPluginService(x, PluginType.SourceControl);
                    return service == null
                        ? SourceControlRetrieveMode.None
                        : service.GetSourceControlRetrieveMode(new[] { x }).Values.First();
                });
        }
    }
}
