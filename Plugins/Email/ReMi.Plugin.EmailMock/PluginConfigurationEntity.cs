using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.EmailMock
{
    public class PluginConfigurationEntity : IPluginConfigurationEntity
    {
        [PluginConfiguration("Redirect to e-mail", PluginConfigurationType.String)]
        public string RedirectToEmail { get; set; }
    }
}
