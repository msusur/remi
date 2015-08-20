using AutoMapper;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Go.DataAccess.DataEntities;

namespace ReMi.Plugin.Go.AutoMapper
{
    public class GoBusinessEntitiesToDataEnities : Profile
    {
        public override string ProfileName
        {
            get { return "GoBusinessEntitiesToDataEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<PluginConfigurationEntity, GlobalConfiguration>()
                .ForMember(target => target.GoServerConfiguration, option => option.Ignore());
            Mapper.CreateMap<NameValuePair, GoServerConfiguration>();
            Mapper.CreateMap<GoPipelineConfiguration, PackageGoPipelineConfiguration>();
        }
    }
}
