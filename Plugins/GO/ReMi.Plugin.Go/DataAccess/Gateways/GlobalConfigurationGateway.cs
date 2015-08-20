using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Plugin.Common.PluginsConfiguration;
using ReMi.Plugin.Go.DataAccess.DataEntities;

namespace ReMi.Plugin.Go.DataAccess.Gateways
{
    public class GlobalConfigurationGateway : BaseGateway, IGlobalConfigurationGateway
    {
        public IRepository<GlobalConfiguration> GlobalConfigurationRepository { get; set; }
        public IRepository<GoServerConfiguration> GoServerConfigurationRepository { get; set; }

        public IMappingEngine Mapper { get; set; }

        public override void OnDisposing()
        {
            GlobalConfigurationRepository.Dispose();
            GoServerConfigurationRepository.Dispose();

            base.OnDisposing();
        }

        public PluginConfigurationEntity GetGlobalConfiguration()
        {
            if (!GlobalConfigurationRepository.Entities.Any())
                GlobalConfigurationRepository.Insert(new GlobalConfiguration());
            return Mapper.Map<GlobalConfiguration, PluginConfigurationEntity>(
                GlobalConfigurationRepository.Entities.First());
        }

        public void SaveGlobalConfiguration(PluginConfigurationEntity globalConfiguration)
        {
            if (!GlobalConfigurationRepository.Entities.Any())
                GlobalConfigurationRepository.Insert(new GlobalConfiguration());
            var configuration = GlobalConfigurationRepository.Entities.First();

            Mapper.Map(globalConfiguration, configuration);
            GlobalConfigurationRepository.Update(configuration);

            var goServers = GoServerConfigurationRepository
                .GetAllSatisfiedBy(x => x.GlobalConfigurationId == configuration.GlobalConfigurationId)
                .ToArray();

            UpdateCollectionHelper.UpdateCollection(goServers, globalConfiguration.GoServers,
                GoServerConfigurationRepository, Mapper,
                (d, b) => d.Name == b.Name,
                d => d.GlobalConfigurationId = configuration.GlobalConfigurationId);
        }
    }
}
