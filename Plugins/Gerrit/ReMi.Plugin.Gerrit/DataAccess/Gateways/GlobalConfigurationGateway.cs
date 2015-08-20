using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Plugin.Gerrit.DataAccess.DataEntities;

namespace ReMi.Plugin.Gerrit.DataAccess.Gateways
{
    public class GlobalConfigurationGateway : BaseGateway, IGlobalConfigurationGateway
    {
        public IRepository<GlobalConfiguration> GlobalConfigurationRepository { get; set; }

        public IMappingEngine Mapper { get; set; }

        public override void OnDisposing()
        {
            GlobalConfigurationRepository.Dispose();

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
        }
    }
}
