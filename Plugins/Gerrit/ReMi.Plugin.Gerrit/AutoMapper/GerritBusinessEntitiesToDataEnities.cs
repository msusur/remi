using AutoMapper;
using ReMi.Plugin.Gerrit.DataAccess.DataEntities;

namespace ReMi.Plugin.Gerrit.AutoMapper
{
    public class GerritBusinessEntitiesToDataEnities : Profile
    {
        public override string ProfileName
        {
            get { return "GerritBusinessEntitiesToDataEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<PluginConfigurationEntity, GlobalConfiguration>();
            Mapper.CreateMap<PluginPackageConfigurationEntity, PackageConfiguration>();
            Mapper.CreateMap<Service.Model.Repository, Repository>();
        }
    }
}
