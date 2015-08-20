using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Plugins;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.DataAccess.BusinessEntityGateways.Plugins
{
    public interface IPluginGateway : IDisposable
    {
        IEnumerable<GlobalPluginConfiguration> GetGlobalPluginConfiguration();
        IEnumerable<PackagePluginConfiguration> GetPackagePluginConfiguration();
        PackagePluginConfiguration GetPackagePluginConfiguration(Guid packageId, PluginType pluginType);
        IEnumerable<BusinessEntities.Plugins.Plugin> GetPlugins();
        BusinessEntities.Plugins.Plugin GetPlugin(string pluginKey); 

        void AddPluginPackageConfiguration(Guid packageId);
        void AssignGlobalPlugin(Guid configurationId, Guid? pluginId);
        void AssignPackagePlugin(Guid configurationId, Guid? pluginId);
    }
}
