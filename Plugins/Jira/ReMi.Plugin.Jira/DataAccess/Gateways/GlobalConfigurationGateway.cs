using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Plugin.Jira.DataAccess.DataEntities;
using System.Linq;

namespace ReMi.Plugin.Jira.DataAccess.Gateways
{
    public class GlobalConfigurationGateway : BaseGateway, IGlobalConfigurationGateway
    {
        public IRepository<GlobalConfiguration> GlobalConfigurationRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

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
        }
    }
}
