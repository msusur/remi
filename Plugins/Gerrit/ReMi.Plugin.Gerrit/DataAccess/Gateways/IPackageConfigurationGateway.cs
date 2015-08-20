using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Gerrit.DataAccess.Gateways
{
    public interface IPackageConfigurationGateway : IDisposable
    {
        PluginPackageConfigurationEntity GetPackageConfiguration(Guid packageId);
        IEnumerable<PluginPackageConfigurationEntity> GetPackagesConfiguration(IEnumerable<Guid> packageIds = null);

        void SavePackageConfiguration(PluginPackageConfigurationEntity packageConfiguration);
    }
}
