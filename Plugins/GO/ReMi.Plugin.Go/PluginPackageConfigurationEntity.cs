using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Go.BusinessLogic;

namespace ReMi.Plugin.Go
{
    public class PluginPackageConfigurationEntity : IPluginPackageConfigurationEntity
    {
        public Guid PackageId { get; set; }

        [PluginConfiguration("GO Server", PluginConfigurationType.Select)]
        public string GoServer { get; set; }

        [PluginConfiguration("GO pipelines", PluginConfigurationType.Json)]
        public IEnumerable<GoPipelineConfiguration> GoPipelines { get; set; }

        [PluginConfiguration("Auto Deploy Time", PluginConfigurationType.Enum)]
        public GettingDeployTimeMode AllowGettingDeployTime { get; set; }

        public string GoServerUrl { get; set; }
    }
}
