using ReMi.Contracts.Plugins.Data;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Plugins;
using ReMi.DataEntities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.BusinessEntities.Plugins;
using ReMi.Common.Utils.Repository;

namespace ReMi.DataAccess.BusinessEntityGateways.Plugins
{
    public class PluginGateway : BaseGateway, IPluginGateway
    {
        public IRepository<Product> ProductRepository { get; set; }
        public IRepository<PluginConfiguration> PluginConfigurationRepository { get; set; }
        public IRepository<PluginPackageConfiguration> PluginPackageConfigurationRepository { get; set; }
        public IRepository<DataEntities.Plugins.Plugin> PluginRepository { get; set; } 
        public IMappingEngine Mapper { get; set; }

        public IEnumerable<GlobalPluginConfiguration> GetGlobalPluginConfiguration()
        {
            var result = PluginConfigurationRepository.Entities;

            return Mapper.Map<IEnumerable<PluginConfiguration>, IEnumerable<GlobalPluginConfiguration>>(result);
        }

        public IEnumerable<PackagePluginConfiguration> GetPackagePluginConfiguration()
        {
            var result = PluginPackageConfigurationRepository.Entities
                .OrderBy(x => x.Package.BusinessUnit.Name)
                .ThenBy(x => x.Package.Description);

            return Mapper.Map<IEnumerable<PluginPackageConfiguration>, IEnumerable<PackagePluginConfiguration>>(result);
        }

        public PackagePluginConfiguration GetPackagePluginConfiguration(Guid packageId, PluginType pluginType)
        {
            var result = PluginPackageConfigurationRepository
                .GetSatisfiedBy(x => x.Package.ExternalId == packageId && x.PluginType == pluginType);

            return Mapper.Map<PluginPackageConfiguration, PackagePluginConfiguration>(result);
        }

        public IEnumerable<BusinessEntities.Plugins.Plugin> GetPlugins()
        {
            var result = PluginRepository.Entities;

            return Mapper.Map<IEnumerable<DataEntities.Plugins.Plugin>, IEnumerable<BusinessEntities.Plugins.Plugin>>(result);
        }

        public BusinessEntities.Plugins.Plugin GetPlugin(string pluginKey)
        {
            var result = PluginRepository.GetSatisfiedBy(x => x.Key == pluginKey);
            if (result == null)
                throw new EntityNotFoundException(typeof(DataEntities.Plugins.Plugin), pluginKey);

            return Mapper.Map<DataEntities.Plugins.Plugin, BusinessEntities.Plugins.Plugin>(result);
        }

        public void AddPluginPackageConfiguration(Guid packageId)
        {
            var product = ProductRepository.GetSatisfiedBy(x => x.ExternalId == packageId);
            if (product == null)
                throw new EntityNotFoundException(typeof(Product), packageId);

            var pluginTypes = PluginPackageConfigurationRepository.Entities
                .Select(x => x.PluginType)
                .Distinct()
                .ToArray();
            PluginPackageConfigurationRepository.Insert(pluginTypes
                .Select(x => new PluginPackageConfiguration
                {
                    ExternalId = Guid.NewGuid(),
                    PluginType = x,
                    PackageId = product.ProductId
                }));
        }

        public void AssignGlobalPlugin(Guid configurationId, Guid? pluginId)
        {
            var configuration = PluginConfigurationRepository.GetSatisfiedBy(x => x.ExternalId == configurationId);
            if (configuration == null)
                throw new EntityNotFoundException(typeof(PluginConfiguration), configurationId);

            var plugin = pluginId.HasValue
                ? PluginRepository.GetSatisfiedBy(x => x.ExternalId == pluginId.Value)
                : null;
            if (pluginId.HasValue && plugin == null)
                throw new EntityNotFoundException(typeof(DataEntities.Plugins.Plugin), pluginId);

            configuration.PluginId = plugin == null ? (int?)null : plugin.PluginId;

            PluginConfigurationRepository.Update(configuration);
        }

        public void AssignPackagePlugin(Guid configurationId, Guid? pluginId)
        {
            var configuration = PluginPackageConfigurationRepository.GetSatisfiedBy(x => x.ExternalId == configurationId);
            if (configuration == null)
                throw new EntityNotFoundException(typeof(PluginPackageConfiguration), configurationId);

            var plugin = pluginId.HasValue
                ? PluginRepository.GetSatisfiedBy(x => x.ExternalId == pluginId.Value)
                : null;
            if (pluginId.HasValue && plugin == null)
                throw new EntityNotFoundException(typeof(DataEntities.Plugins.Plugin), pluginId);

            configuration.PluginId = plugin == null ? (int?)null : plugin.PluginId;

            PluginPackageConfigurationRepository.Update(configuration);
        }

        public override void OnDisposing()
        {
            ProductRepository.Dispose();
            PluginConfigurationRepository.Dispose();
            PluginPackageConfigurationRepository.Dispose();
            PluginRepository.Dispose();

            base.OnDisposing();
        }
    }
}
