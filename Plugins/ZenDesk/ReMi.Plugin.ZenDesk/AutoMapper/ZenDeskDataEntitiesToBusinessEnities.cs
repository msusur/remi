using AutoMapper;
using ReMi.Plugin.ZenDesk.DataAccess.DataEntities;

namespace ReMi.Plugin.ZenDesk.AutoMapper
{
    public class ZenDeskDataEntitiesToBusinessEnities : Profile
    {
        public override string ProfileName
        {
            get { return "ZenDeskDataEntitiesToBusinessEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<GlobalConfiguration, PluginConfigurationEntity>();
            Mapper.CreateMap<PackageConfiguration, PluginPackageConfigurationEntity>();
        }
    }
}
