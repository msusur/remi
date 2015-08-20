using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Jenkins.DataAccess.Gateways
{
    public interface IPackageConfigurationGateway : IDisposable
    {
        PluginPackageConfigurationEntity GetPackageConfiguration(Guid packageId);
        IEnumerable<PluginPackageConfigurationEntity> GetPackagesConfiguration(IEnumerable<Guid> packageIds = null);

        IEnumerable<JenkinsJobConfiguration> GetJobs(IEnumerable<Guid> jobIds);
        IEnumerable<NameValuePair> GetServers();

        void SavePackageConfiguration(PluginPackageConfigurationEntity packageConfiguration);
    }
}
