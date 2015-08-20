using System;

namespace ReMi.Contracts.Plugins.Data
{
    public interface IPluginPackageConfigurationEntity : IPluginConfigurationEntity
    {
        Guid PackageId { get; }
    }
}
