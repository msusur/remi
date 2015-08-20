using ReMi.Contracts.Plugins.Data;
using System;

namespace ReMi.Plugin.ZenDesk
{
    public class PluginPackageConfigurationEntity : IPluginPackageConfigurationEntity
    {
        public Guid PackageId { get; set; }
    }
}
