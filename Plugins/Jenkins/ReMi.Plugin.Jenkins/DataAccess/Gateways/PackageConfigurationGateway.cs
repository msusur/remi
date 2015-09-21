using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Common.PluginsConfiguration;
using ReMi.Plugin.Jenkins.DataAccess.DataEntities;
using ReMi.Plugin.Jenkins.Exceptions;

namespace ReMi.Plugin.Jenkins.DataAccess.Gateways
{
    public class PackageConfigurationGateway : BaseGateway, IPackageConfigurationGateway
    {
        public IRepository<PackageConfiguration> PackageConfigurationRepository { get; set; }
        public IRepository<JenkinsServerConfiguration> JenkinsServerConfigurationRepository { get; set; }
        public IRepository<PackageJenkinsJobConfiguration> PackageJenkinsJobConfigurationRepository { get; set; }
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

        public IEnumerable<JenkinsJobConfiguration> GetJobs(IEnumerable<Guid> jobIds)
        {
            if (!PackageJenkinsJobConfigurationRepository.Entities.Any())
                return null;
            var result = PackageJenkinsJobConfigurationRepository.GetAllSatisfiedBy(x => jobIds.Any(id => id == x.ExternalId));

            return Mapper.Map<IEnumerable<PackageJenkinsJobConfiguration>, IEnumerable<JenkinsJobConfiguration>>(result);
        }

        public IEnumerable<NameValuePair> GetServers()
        {
            if (!JenkinsServerConfigurationRepository.Entities.Any())
                return null;
            var result = JenkinsServerConfigurationRepository.Entities.ToArray();

            return Mapper.Map<IEnumerable<JenkinsServerConfiguration>, IEnumerable<NameValuePair>>(result);
        }

        public void SavePackageConfiguration(PluginPackageConfigurationEntity packageConfiguration)
        {
            if (!packageConfiguration.JenkinsJobs.IsNullOrEmpty())
            {
                var duplicatedId = packageConfiguration.JenkinsJobs
                    .GroupBy(x => x.ExternalId)
                    .FirstOrDefault(x => x.Count() > 1);
                if (duplicatedId != null)
                    throw new JenkinsJobDuplicatedException(duplicatedId.Key);
                var duplicatedName = packageConfiguration.JenkinsJobs
                    .GroupBy(x => x.Name)
                    .FirstOrDefault(x => x.Count() > 1);
                if (duplicatedName != null)
                    throw new JenkinsJobDuplicatedException(duplicatedName.Key);
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

            if (!string.IsNullOrEmpty(packageConfiguration.JenkinsServer))
            {
                var jenkinsServer = JenkinsServerConfigurationRepository
                    .GetSatisfiedBy(x => x.Name == packageConfiguration.JenkinsServer);
                if (jenkinsServer == null)
                    throw new JenkinsServerNotFoundException(packageConfiguration.JenkinsServer);
                configuration.JenkinsServerConfigurationId = jenkinsServer.JenkinsServerConfigurationId;
            }
            else
                configuration.JenkinsServerConfigurationId = null;

            PackageConfigurationRepository.Update(configuration);

            var jobs = PackageJenkinsJobConfigurationRepository
                    .GetAllSatisfiedBy(x => x.PackageConfigurationId == configuration.PackageConfigurationId)
                    .ToArray();

            UpdateCollectionHelper.UpdateCollection(jobs, packageConfiguration.JenkinsJobs,
                PackageJenkinsJobConfigurationRepository, Mapper,
                (d, b) => d.ExternalId == b.ExternalId,
                d => d.PackageConfigurationId = configuration.PackageConfigurationId);
        }

        public override void OnDisposing()
        {
            JenkinsServerConfigurationRepository.Dispose();
            PackageConfigurationRepository.Dispose();

            base.OnDisposing();
        }
    }
}
