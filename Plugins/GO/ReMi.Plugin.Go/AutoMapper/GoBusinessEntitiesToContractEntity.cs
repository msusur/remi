using AutoMapper;
using ReMi.Contracts.Plugins.Data.DeploymentTool;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Plugin.Go.Entities;

namespace ReMi.Plugin.Go.AutoMapper
{
    public class GoBusinessEntitiesToContractEntity : Profile
    {
        public override string ProfileName
        {
            get { return "GoBusinessEntitiesToContractEntity"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<GoPipelineConfiguration, ReleaseJob>()
                .ForMember(target => target.IsIncluded, option => option.MapFrom(source => source.IsIncludedByDefault));

            Mapper.CreateMap<GitCommit, SourceControlChange>()
                .ForMember(target => target.Description, option => option.MapFrom(source => source.Comment))
                .ForMember(target => target.Identifier, option => option.MapFrom(source => source.Revision))
                .ForMember(target => target.Owner, option => option.MapFrom(source => source.User))
                .ForMember(target => target.Repository, option => option.MapFrom(source => source.Name));

            Mapper.CreateMap<StepTiming, JobMetric>()
                .ForMember(target => target.Name, option => option.MapFrom(source => source.StepName))
                .ForMember(target => target.JobId, option => option.MapFrom(source => source.StepId))
                .ForMember(target => target.BuildNumber, option => option.MapFrom(source => source.BuildNumber))
                .ForMember(target => target.ChildMetrics, option => option.MapFrom(source => source.ChildSteps))
                .ForMember(target => target.EndTime, option => option.MapFrom(source => source.FinishTime))
                .ForMember(target => target.NumberOfTries, option => option.MapFrom(source => 1))
                .ForMember(target => target.StartTime, option => option.MapFrom(source => source.StartTime))
                .ForMember(target => target.Url, option => option.MapFrom(source => source.Locator));

        }
    }
}
