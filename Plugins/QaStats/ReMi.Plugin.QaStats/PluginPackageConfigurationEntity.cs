using System;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.QaStats
{
    public class PluginPackageConfigurationEntity : IPluginPackageConfigurationEntity
    {
        public Guid PackageId { get; set; }

        [PluginConfiguration("Qa Service URL", PluginConfigurationType.String)]
        public string PackagePath { get; set; }
    }
}
