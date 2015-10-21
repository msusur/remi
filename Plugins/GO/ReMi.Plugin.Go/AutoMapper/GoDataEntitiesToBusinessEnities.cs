using AutoMapper;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Go.BusinessLogic;
using ReMi.Plugin.Go.DataAccess.DataEntities;

namespace ReMi.Plugin.Go.AutoMapper
{
    public class GoDataEntitiesToBusinessEnities : Profile
    {
        public override string ProfileName
        {
            get { return "GoDataEntitiesToBusinessEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<GlobalConfiguration, PluginConfigurationEntity>()
                .ForMember(target => target.GoServers, option => option.MapFrom(source => source.GoServerConfiguration));
            Mapper.CreateMap<GoServerConfiguration, NameValuePair>();
            Mapper.CreateMap<PackageConfiguration, PluginPackageConfigurationEntity>()
                .ForMember(target => target.GoServer, option => option.MapFrom(source => source.GoServerConfiguration == null ? null : source.GoServerConfiguration.Name))
                .ForMember(target => target.GoServerUrl, option => option.MapFrom(source => source.GoServerConfiguration == null ? null : source.GoServerConfiguration.Value))
                .ForMember(target => target.GoPipelines, option => option.MapFrom(source => source.PackageGoPipelineConfiguration))
                .ForMember(target => target.AllowGettingDeployTime, option => option.MapFrom(
                    source => source.AllowGettingDeployTime ? GettingDeployTimeMode.Allow : GettingDeployTimeMode.Forbid));
            Mapper.CreateMap<PackageGoPipelineConfiguration, GoPipelineConfiguration>();
        }
    }
}
