using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Plugin.Common.PluginsConfiguration;
using ReMi.Plugin.ZenDesk.DataAccess.Gateways;
using System;
using System.Collections.Generic;

namespace ReMi.Plugin.ZenDesk
{
    public class PluginPackageConfiguration : IPluginPackageConfiguration<PluginPackageConfigurationEntity>
    {
        public Func<IPackageConfigurationGateway> PackageConfigurationGatewayFactory { get; set; }
        public ISerialization Serialization { get; set; }

        public IEnumerable<PluginPackageConfigurationEntity> GetPluginPackageConfiguration()
        {
            return null;
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                return gateway.GetPackagesConfiguration();
            }
        }

        public PluginPackageConfigurationEntity GetPluginPackageConfigurationEntity(Guid packageId)
        {
            return null;
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                return gateway.GetPackageConfiguration(packageId);
            }
        }

        IEnumerable<IPluginPackageConfigurationEntity> IPluginPackageConfiguration.GetPluginPackageConfiguration()
        {
            return GetPluginPackageConfiguration();
        }

        IPluginPackageConfigurationEntity IPluginPackageConfiguration.GetPluginPackageConfigurationEntity(Guid packageId)
        {
            return GetPluginPackageConfigurationEntity(packageId);
        }

        public void SetPluginPackageConfigurationEntity(Guid packageId,
            string pluginPackageConfiguration)
        {
            return;
            var configurationEntity = Serialization.FromJson<PluginPackageConfigurationEntity>(pluginPackageConfiguration);
            configurationEntity.PackageId = packageId;
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                gateway.SavePackageConfiguration(configurationEntity);
            }
        }

        public void SetPluginPackageConfigurationEntity(Guid packageId, string propertyName, string value)
        {
            return;
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                var configurationEntity = gateway.GetPackageConfiguration(packageId);
                configurationEntity.UpdatePropertyValue(propertyName, value, Serialization);
                configurationEntity.PackageId = packageId;
                gateway.SavePackageConfiguration(configurationEntity);
            }
        }

        public IEnumerable<PluginConfigurationTemplate> GetConfigurationTemplate()
        {
            return null;
            return PluginConfigurationTemplateBuilder.Build<PluginPackageConfigurationEntity>();
        }
    }
}
