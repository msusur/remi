using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Plugin.QaStats.DataAccess.DataEntities;

namespace ReMi.Plugin.QaStats.DataAccess.Gateways
{
    public class PackageConfigurationGateway : BaseGateway, IPackageConfigurationGateway
    {
        public IRepository<PackageConfiguration> PackageConfigurationRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public PluginPackageConfigurationEntity GetPackageConfiguration(Guid packageId)
        {
            var configuration = PackageConfigurationRepository
                .GetSatisfiedBy(x => x.PackageId == packageId);

            return configuration == null
                ? null
                : Mapper.Map<PackageConfiguration, PluginPackageConfigurationEntity>(configuration);
        }

        public IEnumerable<PluginPackageConfigurationEntity> GetPackagesConfiguration()
        {
            if (!PackageConfigurationRepository.Entities.Any())
                return Enumerable.Empty<PluginPackageConfigurationEntity>();

            return Mapper.Map<IEnumerable<PackageConfiguration>, IEnumerable<PluginPackageConfigurationEntity>>(
                PackageConfigurationRepository.Entities);
        }

        public void SavePackageConfiguration(PluginPackageConfigurationEntity packageConfiguration)
        {
            if (!PackageConfigurationRepository.Entities
                .Any(x => x.PackageId == packageConfiguration.PackageId))
            {
                PackageConfigurationRepository.Insert(new PackageConfiguration
                {
                    PackageId = packageConfiguration.PackageId
                });
            }

            var configuration = PackageConfigurationRepository
                .GetSatisfiedBy(x => x.PackageId == packageConfiguration.PackageId);
            configuration.PackagePath = packageConfiguration.PackagePath;
            PackageConfigurationRepository.Update(configuration);
        }
    }
}
