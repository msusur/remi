using AutoMapper;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Jira.DataAccess.DataEntities;

namespace ReMi.Plugin.Jira.AutoMapper
{
    public class JiraBusinessEnitiesToDataEntities : Profile
    {
        public override string ProfileName
        {
            get { return "JiraBusinessEnitiesToDataEntities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<PluginConfigurationEntity, GlobalConfiguration>();
            Mapper.CreateMap<PluginPackageConfigurationEntity, PackageConfiguration>()
                .ForMember(target => target.PackageJqlFilters, option => option.MapFrom(source => source.JqlFilter))
                .ForMember(target => target.PackageDefectJqlFilters, option => option.MapFrom(source => source.DefectFilter));
            Mapper.CreateMap<NameValuePair, PackageJqlFilter>();
            Mapper.CreateMap<NameValuePair, PackageDefectJqlFilter>();
        }
    }
}
