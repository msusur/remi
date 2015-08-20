using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Plugin.Common.PluginsConfiguration;
using ReMi.Plugin.Jenkins.DataAccess.DataEntities;

namespace ReMi.Plugin.Jenkins.DataAccess.Gateways
{
    public class GlobalConfigurationGateway : BaseGateway, IGlobalConfigurationGateway
    {
        public IRepository<GlobalConfiguration> GlobalConfigurationRepository { get; set; }
        public IRepository<JenkinsServerConfiguration> JenkinsServerConfigurationRepository { get; set; }

        public IMappingEngine Mapper { get; set; }

        public override void OnDisposing()
        {
            GlobalConfigurationRepository.Dispose();
            JenkinsServerConfigurationRepository.Dispose();

            base.OnDisposing();
        }

        public PluginConfigurationEntity GetGlobalConfiguration()
        {
            if (!GlobalConfigurationRepository.Entities.Any())
                GlobalConfigurationRepository.Insert(new GlobalConfiguration());
            var config = GlobalConfigurationRepository.Entities.First();
            var result = Mapper.Map<GlobalConfiguration, PluginConfigurationEntity>(config);
            return result;
        }

        public void SaveGlobalConfiguration(PluginConfigurationEntity globalConfiguration)
        {
            if (!GlobalConfigurationRepository.Entities.Any())
                GlobalConfigurationRepository.Insert(new GlobalConfiguration());
            var configuration = GlobalConfigurationRepository.Entities.First();

            Mapper.Map(globalConfiguration, configuration);
            GlobalConfigurationRepository.Update(configuration);

            var jenkinsServers = JenkinsServerConfigurationRepository
                .GetAllSatisfiedBy(x => x.GlobalConfigurationId == configuration.GlobalConfigurationId)
                .ToArray();

            UpdateCollectionHelper.UpdateCollection(jenkinsServers, globalConfiguration.JenkinsServers,
                JenkinsServerConfigurationRepository, Mapper,
                (d, b) => d.Name == b.Name,
                d => d.GlobalConfigurationId = configuration.GlobalConfigurationId);
        }
    }
}
