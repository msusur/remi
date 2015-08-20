using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Contracts.Plugins.Services
{
    public interface IPluginPackageConfiguration<out T> : IPluginPackageConfiguration
        where T : IPluginPackageConfigurationEntity, new()
    {
        new IEnumerable<T> GetPluginPackageConfiguration();
        new T GetPluginPackageConfigurationEntity(Guid packageId);
    }
}
