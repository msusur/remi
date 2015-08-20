using AutoMapper;
using ReMi.Plugin.ZenDesk.DataAccess.DataEntities;

namespace ReMi.Plugin.ZenDesk.AutoMapper
{
    public class ZenDeskBusinessEnitiesToDataEntities : Profile
    {
        public override string ProfileName
        {
            get { return "ZenDeskBusinessEnitiesToDataEntities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<PluginConfigurationEntity, GlobalConfiguration>();
            Mapper.CreateMap<PluginPackageConfigurationEntity, PackageConfiguration>();
        }
    }
}
