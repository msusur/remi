using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Plugin.Common.PluginsConfiguration;
using ReMi.Plugin.Gerrit.DataAccess.Gateways;

namespace ReMi.Plugin.Gerrit
{
    public class PluginPackageConfiguration : IPluginPackageConfiguration<PluginPackageConfigurationEntity>
    {
        public Func<IPackageConfigurationGateway> PackageConfigurationGatewayFactory { get; set; }
        public ISerialization Serialization { get; set; }

        public IEnumerable<PluginPackageConfigurationEntity> GetPluginPackageConfiguration()
        {
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                return gateway.GetPackagesConfiguration();
            }
        }

        public PluginPackageConfigurationEntity GetPluginPackageConfigurationEntity(Guid packageId)
        {
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
            var configurationEntity = Serialization.FromJson<PluginPackageConfigurationEntity>(pluginPackageConfiguration);
            configurationEntity.PackageId = packageId;
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                gateway.SavePackageConfiguration(configurationEntity);
            }
        }

        public void SetPluginPackageConfigurationEntity(Guid packageId, string propertyName, string value)
        {
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
            return PluginConfigurationTemplateBuilder.Build<PluginPackageConfigurationEntity>().ToArray();
        }
    }
}
