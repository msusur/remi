using System;
using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Plugin.Common.PluginsConfiguration;
using ReMi.Plugin.Gerrit.DataAccess.Gateways;
using IPluginConfiguration = ReMi.Contracts.Plugins.Services.IPluginConfiguration;

namespace ReMi.Plugin.Gerrit
{
    public class PluginConfiguration : IPluginConfiguration<PluginConfigurationEntity>
    {
        public Func<IGlobalConfigurationGateway> GlobalConfigurationGatewayFactory { get; set; } 
        public ISerialization Serialization { get; set; }

        public PluginConfigurationEntity GetPluginConfiguration()
        {
            using (var gateway = GlobalConfigurationGatewayFactory())
            {
                return gateway.GetGlobalConfiguration();
            }
        }

        IPluginConfigurationEntity IPluginConfiguration.GetPluginConfiguration()
        {
            return GetPluginConfiguration();
        }

        public void SetPluginConfiguration(string configuration)
        {
            var configurationEntity = Serialization.FromJson<PluginConfigurationEntity>(configuration);
            using (var gateway = GlobalConfigurationGatewayFactory())
            {
                gateway.SaveGlobalConfiguration(configurationEntity);
            }
        }

        public void SetPluginConfiguration(string propertyName, string value)
        {
            using (var gateway = GlobalConfigurationGatewayFactory())
            {
                var configurationEntity = gateway.GetGlobalConfiguration();
                configurationEntity.UpdatePropertyValue(propertyName, value, Serialization);
                gateway.SaveGlobalConfiguration(configurationEntity);
            }
        }

        public IEnumerable<PluginConfigurationTemplate> GetConfigurationTemplate()
        {
            return PluginConfigurationTemplateBuilder.Build<PluginConfigurationEntity>();
        }
    }
}
