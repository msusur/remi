using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Contracts.Plugins.Services
{
    public interface IPluginConfiguration
    {
        IPluginConfigurationEntity GetPluginConfiguration();

        void SetPluginConfiguration(string configuration);
        void SetPluginConfiguration(string propertyName, string value);

        IEnumerable<PluginConfigurationTemplate> GetConfigurationTemplate();
    }
}
