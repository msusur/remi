using System;
using AutoMapper;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.HelpDesk;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Plugins.Data.HelpDesk;

namespace ReMi.BusinessLogic.AutoMapper
{
    public class ContractEntityToBusinessEntityMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ContractEntityToBusinessEntityMappingProfile"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket, ReleaseContentTicket>();

            Mapper.CreateMap<Contracts.Plugins.Data.DeploymentTool.ReleaseJob, ReleaseJob>()
                .ForMember(target => target.JobId, options => options.MapFrom(source => source.ExternalId))
                .ForMember(target => target.ExternalId, options => options.MapFrom(source => Guid.NewGuid()));

            Mapper.CreateMap<Contracts.Plugins.Data.DeploymentTool.JobMetric, JobMeasurement>()
                .ForMember(target => target.ChildSteps, options => options.MapFrom(source => source.ChildMetrics))
                .ForMember(target => target.FinishTime, options => options.MapFrom(source => source.EndTime))
                .ForMember(target => target.Locator, options => options.MapFrom(source => source.Url))
                .ForMember(target => target.StartTime, options => options.MapFrom(source => source.StartTime))
                .ForMember(target => target.BuildNumber, options => options.MapFrom(source => source.BuildNumber))
                .ForMember(target => target.StepId, options => options.MapFrom(source => source.JobId))
                .ForMember(target => target.StepName, options => options.MapFrom(source => source.Name));

            Mapper.CreateMap<HelpDeskTicket, HelpDeskTaskView>()
                .ForMember(x => x.Number, o => o.MapFrom(x => x.Id))
                .ForMember(x => x.Description, o => o.MapFrom(x => x.Subject))
                .ForMember(x => x.LinkUrl, o => o.MapFrom(x => x.Url));

            Mapper.CreateMap<HelpDeskTicket, HelpDeskTask>()
                .ForMember(x => x.Number, o => o.MapFrom(x => x.Id))
                .ForMember(x => x.Subject, o => o.MapFrom(x => x.Subject))
                .ForMember(x => x.Description, o => o.MapFrom(x => x.Description))
                .ForMember(x => x.Url, o => o.MapFrom(x => x.Url));
        }
    }
}
