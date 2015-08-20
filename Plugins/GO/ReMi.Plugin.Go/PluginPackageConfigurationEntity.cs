using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Go
{
    public class PluginPackageConfigurationEntity : IPluginPackageConfigurationEntity
    {
        public Guid PackageId { get; set; }

        [PluginConfiguration("GO Server", PluginConfigurationType.Select)]
        public string GoServer { get; set; }

        [PluginConfiguration("GO pipelines", PluginConfigurationType.Json)]
        public IEnumerable<GoPipelineConfiguration> GoPipelines { get; set; }

        public string GoServerUrl { get; set; }
    }
}
