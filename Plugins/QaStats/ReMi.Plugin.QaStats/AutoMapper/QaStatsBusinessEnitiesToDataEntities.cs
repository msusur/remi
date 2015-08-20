using AutoMapper;
using ReMi.Plugin.QaStats.DataAccess.DataEntities;

namespace ReMi.Plugin.QaStats.AutoMapper
{
    public class QaStatsBusinessEnitiesToDataEntities : Profile
    {
        public override string ProfileName
        {
            get { return "QaStatsBusinessEnitiesToDataEntities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<PluginConfigurationEntity, GlobalConfiguration>();
            Mapper.CreateMap<PluginPackageConfigurationEntity, PackageConfiguration>();
        }
    }
}
