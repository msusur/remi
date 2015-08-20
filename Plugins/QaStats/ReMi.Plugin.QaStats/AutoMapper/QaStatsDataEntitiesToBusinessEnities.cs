using AutoMapper;
using ReMi.Plugin.QaStats.DataAccess.DataEntities;

namespace ReMi.Plugin.QaStats.AutoMapper
{
    public class QaStatsDataEntitiesToBusinessEnities : Profile
    {
        public override string ProfileName
        {
            get { return "QaStatsDataEntitiesToBusinessEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<GlobalConfiguration, PluginConfigurationEntity>();
            Mapper.CreateMap<PackageConfiguration, PluginPackageConfigurationEntity>();
        }
    }
}
