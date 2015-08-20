using AutoMapper;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Jira.DataAccess.DataEntities;

namespace ReMi.Plugin.Jira.AutoMapper
{
    public class JiraDataEntitiesToBusinessEnities : Profile
    {
        public override string ProfileName
        {
            get { return "JiraDataEntitiesToBusinessEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<GlobalConfiguration, PluginConfigurationEntity>();
            Mapper.CreateMap<PackageConfiguration, PluginPackageConfigurationEntity>()
                .ForMember(target => target.JqlFilter, option => option.MapFrom(source => source.PackageJqlFilters))
                .ForMember(target => target.DefectFilter, option => option.MapFrom(source => source.PackageDefectJqlFilters));
            Mapper.CreateMap<PackageJqlFilter, NameValuePair>();
            Mapper.CreateMap<PackageDefectJqlFilter, NameValuePair>();
        }
    }
}
