using ReMi.Common.Utils.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Jira.DataAccess.DataEntities;

namespace ReMi.Plugin.Jira.DataAccess.Gateways
{
    public class PackageConfigurationGateway : BaseGateway, IPackageConfigurationGateway
    {
        public IRepository<PackageConfiguration> PackageConfigurationRepository { get; set; }
        public IRepository<PackageJqlFilter> PackageJqlFilterRepository { get; set; }
        public IRepository<PackageDefectJqlFilter> PackageDefectJqlFilterRepository { get; set; }
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
                return null;

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
            configuration.Label = packageConfiguration.Label;
            configuration.UpdateTicketMode = packageConfiguration.UpdateTicketMode;
            PackageConfigurationRepository.Update(configuration);

            var filters = PackageJqlFilterRepository
                    .GetAllSatisfiedBy(x => x.PackageConfigurationId == configuration.PackageConfigurationId)
                    .ToArray();
            var defectFilters = PackageDefectJqlFilterRepository
                    .GetAllSatisfiedBy(x => x.PackageConfigurationId == configuration.PackageConfigurationId)
                    .ToArray();

            var toUpdateFilters = filters.
                Where(x => packageConfiguration.JqlFilter.Any(f => x.Name == f.Name)).ToList();
            var toRemoveFilters = filters.
                Where(x => packageConfiguration.JqlFilter.All(f => x.Name != f.Name)).ToList();
            var toInsertFilters = packageConfiguration.JqlFilter.
                Where(x => filters.All(f => x.Name != f.Name)).ToList();

            toUpdateFilters.Each(x =>
            {
                x.Value = packageConfiguration.JqlFilter.First(f => f.Name == x.Name).Value;
                PackageJqlFilterRepository.Update(x);
            });
            toRemoveFilters.Each(PackageJqlFilterRepository.Delete);
            toInsertFilters.Each(x =>
            {
                var filter = Mapper.Map<NameValuePair, PackageJqlFilter>(
                        packageConfiguration.JqlFilter.First(f => f.Name == x.Name));
                filter.PackageConfigurationId = configuration.PackageConfigurationId;
                PackageJqlFilterRepository.Insert(filter);
            });

            var toUpdateDefectFilters = defectFilters.
                Where(x => packageConfiguration.DefectFilter.Any(f => x.Name == f.Name)).ToList();
            var toRemoveDefectFilters = defectFilters.
                Where(x => packageConfiguration.DefectFilter.All(f => x.Name != f.Name)).ToList();
            var toInsertDefectFilters = packageConfiguration.DefectFilter.
                Where(x => defectFilters.All(f => x.Name != f.Name)).ToList();

            toUpdateDefectFilters.Each(x =>
            {
                x.Value = packageConfiguration.DefectFilter.First(f => f.Name == x.Name).Value;
                PackageDefectJqlFilterRepository.Update(x);
            });
            toRemoveDefectFilters.Each(PackageDefectJqlFilterRepository.Delete);
            toInsertDefectFilters.Each(x =>
            {
                var filter = Mapper.Map<NameValuePair, PackageDefectJqlFilter>(
                    packageConfiguration.DefectFilter.First(f => f.Name == x.Name));
                filter.PackageConfigurationId = configuration.PackageConfigurationId;
                PackageDefectJqlFilterRepository.Insert(filter);
            });

        }
    }
}
