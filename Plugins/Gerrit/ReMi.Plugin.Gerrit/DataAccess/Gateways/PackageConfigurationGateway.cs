using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Plugin.Gerrit.DataAccess.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils;
using ReMi.Plugin.Common.PluginsConfiguration;

namespace ReMi.Plugin.Gerrit.DataAccess.Gateways
{
    public class PackageConfigurationGateway : BaseGateway, IPackageConfigurationGateway
    {
        public IRepository<PackageConfiguration> PackageConfigurationRepository { get; set; }
        public IRepository<Repository> Repository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public PluginPackageConfigurationEntity GetPackageConfiguration(Guid packageId)
        {
            var configuration = PackageConfigurationRepository
                .GetSatisfiedBy(x => x.PackageId == packageId);

            return configuration == null
                ? null
                : Mapper.Map<PackageConfiguration, PluginPackageConfigurationEntity>(configuration);
        }

        public IEnumerable<PluginPackageConfigurationEntity> GetPackagesConfiguration(IEnumerable<Guid> packageIds = null)
        {
            if (!PackageConfigurationRepository.Entities.Any())
                return null;
            var result = packageIds == null
                ? PackageConfigurationRepository.Entities
                : PackageConfigurationRepository.GetAllSatisfiedBy(x => packageIds.Any(id => id == x.PackageId));

            return Mapper.Map<IEnumerable<PackageConfiguration>, IEnumerable<PluginPackageConfigurationEntity>>(result);
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

            PackageConfigurationRepository.Update(configuration);

            if (packageConfiguration.Repositories.IsNullOrEmpty())
                return;

            var repositories = Repository
                .GetAllSatisfiedBy(x => x.PackageConfigurationId == configuration.PackageConfigurationId)
                .ToArray();

            UpdateCollectionHelper.UpdateCollection(repositories, packageConfiguration.Repositories,
                Repository, Mapper,
                (d, b) => d.ExternalId == b.ExternalId,
                d => d.PackageConfigurationId = configuration.PackageConfigurationId);

        }

        public override void OnDisposing()
        {
            PackageConfigurationRepository.Dispose();

            base.OnDisposing();
        }
    }
}
