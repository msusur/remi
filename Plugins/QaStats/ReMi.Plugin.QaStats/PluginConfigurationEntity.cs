using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.QaStats
{
    public class PluginConfigurationEntity : IPluginConfigurationEntity
    {
        [PluginConfiguration("QA Sevice base URL", PluginConfigurationType.String)]
        public string QaServiceUrl { get; set; }
    }
}
