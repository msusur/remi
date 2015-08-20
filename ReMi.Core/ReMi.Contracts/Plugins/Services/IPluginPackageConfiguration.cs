using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Contracts.Plugins.Services
{
    public interface IPluginPackageConfiguration
    {
        IEnumerable<IPluginPackageConfigurationEntity> GetPluginPackageConfiguration();
        IPluginPackageConfigurationEntity GetPluginPackageConfigurationEntity(Guid packageId);

        void SetPluginPackageConfigurationEntity(Guid packageId, string pluginPackageConfiguration);
        void SetPluginPackageConfigurationEntity(Guid packageId, string propertyName, string value);

        IEnumerable<PluginConfigurationTemplate> GetConfigurationTemplate();
    }
}
