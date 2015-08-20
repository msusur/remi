using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Plugin.Common.PluginsConfiguration;
using System;
using System.Collections.Generic;
using IPluginConfiguration = ReMi.Contracts.Plugins.Services.IPluginConfiguration;

namespace ReMi.Plugin.Email
{
    public class PluginConfiguration : IPluginConfiguration<PluginConfigurationEntity>
    {
        private readonly PluginConfigurationEntity configuration;

        public PluginConfiguration()
        {
            configuration = new PluginConfigurationEntity();
        }

        public PluginConfigurationEntity GetPluginConfiguration()
        {
            return configuration;
        }

        IPluginConfigurationEntity IPluginConfiguration.GetPluginConfiguration()
        {
            return GetPluginConfiguration();
        }

        public void SetPluginConfiguration(string configuration)
        {
            throw new NotImplementedException();
        }

        public void SetPluginConfiguration(string propertyName, string value)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PluginConfigurationTemplate> GetConfigurationTemplate()
        {
            return PluginConfigurationTemplateBuilder.Build<PluginConfigurationEntity>();
        }
    }
}
