using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Common.PluginsConfiguration;
using ReMi.Plugin.Go.BusinessLogic;
using ReMi.Plugin.Go.DataAccess.DataEntities;
using ReMi.Plugin.Go.Exceptions;

namespace ReMi.Plugin.Go.DataAccess.Gateways
{
    public class PackageConfigurationGateway : BaseGateway, IPackageConfigurationGateway
    {
        public IRepository<PackageConfiguration> PackageConfigurationRepository { get; set; }
        public IRepository<GoServerConfiguration> GoServerConfigurationRepository { get; set; }
        public IRepository<PackageGoPipelineConfiguration> PackageGoPipelineConfigurationRepository { get; set; }
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
                return Enumerable.Empty<PluginPackageConfigurationEntity>();

            var result = packageIds == null
                ? PackageConfigurationRepository.Entities
                : PackageConfigurationRepository.GetAllSatisfiedBy(x => packageIds.Any(id => id == x.PackageId));

            return Mapper.Map<IEnumerable<PackageConfiguration>, IEnumerable<PluginPackageConfigurationEntity>>(result);
        }

        public IEnumerable<GoPipelineConfiguration> GetPipelines(IEnumerable<Guid> jobIds)
        {
            if (!PackageGoPipelineConfigurationRepository.Entities.Any())
                return null;
            var result = PackageGoPipelineConfigurationRepository.GetAllSatisfiedBy(x => jobIds.Any(id => id == x.ExternalId));

            return Mapper.Map<IEnumerable<PackageGoPipelineConfiguration>, IEnumerable<GoPipelineConfiguration>>(result);
        }

        public IEnumerable<NameValuePair> GetServers()
        {
            if (!GoServerConfigurationRepository.Entities.Any())
                return null;
            var result = GoServerConfigurationRepository.Entities.ToArray();

            return Mapper.Map<IEnumerable<GoServerConfiguration>, IEnumerable<NameValuePair>>(result);
        }

        public void SavePackageConfiguration(PluginPackageConfigurationEntity packageConfiguration)
        {
            if (!packageConfiguration.GoPipelines.IsNullOrEmpty())
            {
                var duplicatedId = packageConfiguration.GoPipelines
                    .GroupBy(x => x.ExternalId)
                    .FirstOrDefault(x => x.Count() > 1);
                if (duplicatedId != null)
                    throw new GoPipelineDuplicatedException(duplicatedId.Key);
                var duplicatedName = packageConfiguration.GoPipelines
                    .GroupBy(x => x.Name)
                    .FirstOrDefault(x => x.Count() > 1);
                if (duplicatedName != null)
                    throw new GoPipelineDuplicatedException(duplicatedName.Key);
            }
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
            configuration.AllowGettingDeployTime
                = packageConfiguration.AllowGettingDeployTime == GettingDeployTimeMode.Allow;

            if (!string.IsNullOrEmpty(packageConfiguration.GoServer))
            {
                var goServer = GoServerConfigurationRepository
                    .GetSatisfiedBy(x => x.Name == packageConfiguration.GoServer);
                if (goServer == null)
                    throw new GoServerNotFoundException(packageConfiguration.GoServer);
                configuration.GoServerConfigurationId = goServer.GoServerConfigurationId;
            }
            else
                configuration.GoServerConfigurationId = null;

            PackageConfigurationRepository.Update(configuration);

            var pipelines = PackageGoPipelineConfigurationRepository
                    .GetAllSatisfiedBy(x => x.PackageConfigurationId == configuration.PackageConfigurationId)
                    .ToArray();

            UpdateCollectionHelper.UpdateCollection(pipelines, packageConfiguration.GoPipelines,
                PackageGoPipelineConfigurationRepository, Mapper,
                (d, b) => d.ExternalId == b.ExternalId,
                d => d.PackageConfigurationId = configuration.PackageConfigurationId);
        }

        public override void OnDisposing()
        {
            GoServerConfigurationRepository.Dispose();
            PackageConfigurationRepository.Dispose();

            base.OnDisposing();
        }
    }
}
