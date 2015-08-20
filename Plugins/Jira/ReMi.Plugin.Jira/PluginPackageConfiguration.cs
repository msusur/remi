using System.Linq;
using ReMi.Contracts.Plugins.Services;
using System;
using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Common.PluginsConfiguration;
using ReMi.Plugin.Jira.DataAccess.Gateways;

namespace ReMi.Plugin.Jira
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
            if (configurationEntity.JqlFilter == null)
                configurationEntity.JqlFilter = Enumerable.Empty<NameValuePair>();
            if (configurationEntity.DefectFilter == null)
                configurationEntity.DefectFilter = Enumerable.Empty<NameValuePair>();
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
                if (configurationEntity.JqlFilter == null)
                    configurationEntity.JqlFilter = Enumerable.Empty<NameValuePair>();
                if (configurationEntity.DefectFilter == null)
                    configurationEntity.DefectFilter = Enumerable.Empty<NameValuePair>();
                gateway.SavePackageConfiguration(configurationEntity);
            }
        }


        public IEnumerable<PluginConfigurationTemplate> GetConfigurationTemplate()
        {
            return PluginConfigurationTemplateBuilder.Build<PluginPackageConfigurationEntity>();
        }
    }
}
