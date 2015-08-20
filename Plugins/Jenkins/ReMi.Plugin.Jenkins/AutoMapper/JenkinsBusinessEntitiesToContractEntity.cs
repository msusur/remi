using AutoMapper;
using ReMi.Contracts.Plugins.Data.DeploymentTool;

namespace ReMi.Plugin.Jenkins.AutoMapper
{
    public class JenkinsBusinessEntitiesToContractEntity : Profile
    {
        public override string ProfileName
        {
            get { return "JenkinsBusinessEntitiesToContractEntity"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<JenkinsJobConfiguration, ReleaseJob>()
                .ForMember(target => target.IsIncluded, option => option.MapFrom(source => source.IsIncludedByDefault));
        }
    }
}
