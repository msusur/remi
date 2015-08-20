using AutoMapper;
using ReMi.Plugin.Gerrit.DataAccess.DataEntities;

namespace ReMi.Plugin.Gerrit.AutoMapper
{
    public class GerritDataEntitiesToBusinessEnities : Profile
    {
        public override string ProfileName
        {
            get { return "GerritDataEntitiesToBusinessEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<GlobalConfiguration, PluginConfigurationEntity>();
            Mapper.CreateMap<PackageConfiguration, PluginPackageConfigurationEntity>();
            Mapper.CreateMap<Repository, Service.Model.Repository>();
        }
    }
}
