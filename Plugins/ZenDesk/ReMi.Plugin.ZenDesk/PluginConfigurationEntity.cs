using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.ZenDesk
{
    public class PluginConfigurationEntity : IPluginConfigurationEntity
    {
        [PluginConfiguration("ZenDesk URL", PluginConfigurationType.String)]
        public string ZenDeskUrl { get; set; }
        [PluginConfiguration("ZenDesk User Name", PluginConfigurationType.String)]
        public string ZenDeskUser { get; set; }
        [PluginConfiguration("ZenDesk Password", PluginConfigurationType.Password)]
        public string ZenDeskPassword { get; set; }
    }
}
