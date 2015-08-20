using AutoMapper;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Jenkins.DataAccess.DataEntities;

namespace ReMi.Plugin.Jenkins.AutoMapper
{
    public class JenkinsDataEntitiesToBusinessEnities : Profile
    {
        public override string ProfileName
        {
            get { return "JenkinsDataEntitiesToBusinessEnitiess"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<GlobalConfiguration, PluginConfigurationEntity>()
                .ForMember(target => target.JenkinsServers, option => option.MapFrom(source => source.JenkinsServers));
            Mapper.CreateMap<JenkinsServerConfiguration, NameValuePair>();
            Mapper.CreateMap<PackageConfiguration, PluginPackageConfigurationEntity>()
                .ForMember(target => target.JenkinsServer, option => option.MapFrom(source => source.JenkinsServerConfiguration == null ? null : source.JenkinsServerConfiguration.Name))
                .ForMember(target => target.JenkinsServerUrl, option => option.MapFrom(source => source.JenkinsServerConfiguration == null ? null : source.JenkinsServerConfiguration.Value))
                .ForMember(target => target.JenkinsJobs, option => option.MapFrom(source => source.PackageJenkinsJobConfiguration));
            Mapper.CreateMap<PackageJenkinsJobConfiguration, JenkinsJobConfiguration>();
        }
    }
}
