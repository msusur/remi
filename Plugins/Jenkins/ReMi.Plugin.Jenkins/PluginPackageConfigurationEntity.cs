using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Jenkins.BusinessLogic;

namespace ReMi.Plugin.Jenkins
{
    public class PluginPackageConfigurationEntity : IPluginPackageConfigurationEntity
    {
        public Guid PackageId { get; set; }

        [PluginConfiguration("Jenkins Server", PluginConfigurationType.Select)]
        public string JenkinsServer { get; set; }

        [PluginConfiguration("Jenkins jobs", PluginConfigurationType.Json)]
        public IEnumerable<JenkinsJobConfiguration> JenkinsJobs { get; set; }

        [PluginConfiguration("Time zone", PluginConfigurationType.Enum)]
        public TimeZone TimeZone { get; set; }

        [PluginConfiguration("Auto Deploy Time", PluginConfigurationType.Enum)]
        public GettingDeployTimeMode AllowGettingDeployTime { get; set; }

        public string JenkinsServerUrl { get; set; }
    }
}
