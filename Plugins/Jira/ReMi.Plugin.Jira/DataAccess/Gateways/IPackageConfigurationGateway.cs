using System;
using System.Collections.Generic;

namespace ReMi.Plugin.Jira.DataAccess.Gateways
{
    public interface IPackageConfigurationGateway : IDisposable
    {
        PluginPackageConfigurationEntity GetPackageConfiguration(Guid packageId);
        IEnumerable<PluginPackageConfigurationEntity> GetPackagesConfiguration();

        void SavePackageConfiguration(PluginPackageConfigurationEntity packageConfiguration);
    }
}
