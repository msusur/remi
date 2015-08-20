using AutoMapper;
using ReMi.Contracts.Plugins.Data;
using ReMi.Plugin.Jenkins.DataAccess.DataEntities;

namespace ReMi.Plugin.Jenkins.AutoMapper
{
    public class JenkinsBusinessEntitiesToDataEnities : Profile
    {
        public override string ProfileName
        {
            get { return "JenkinsBusinessEntitiesToDataEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<PluginConfigurationEntity, GlobalConfiguration>()
                .ForMember(target => target.JenkinsServers, options => options.Ignore());
            Mapper.CreateMap<NameValuePair, JenkinsServerConfiguration>();
            Mapper.CreateMap<JenkinsJobConfiguration, PackageJenkinsJobConfiguration>();
            Mapper.CreateMap<PluginPackageConfigurationEntity, PackageConfiguration>();
        }
    }
}
