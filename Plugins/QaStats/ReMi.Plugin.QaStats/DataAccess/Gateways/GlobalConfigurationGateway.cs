using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Plugin.QaStats.DataAccess.DataEntities;

namespace ReMi.Plugin.QaStats.DataAccess.Gateways
{
    public class GlobalConfigurationGateway : BaseGateway, IGlobalConfigurationGateway
    {
        public IRepository<GlobalConfiguration> GlobalConfigurationRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public PluginConfigurationEntity GetGlobalConfiguration()
        {
            var result = GlobalConfigurationRepository.Entities.FirstOrDefault();
            return result == null ? null : Mapper.Map<GlobalConfiguration, PluginConfigurationEntity>(result);
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
