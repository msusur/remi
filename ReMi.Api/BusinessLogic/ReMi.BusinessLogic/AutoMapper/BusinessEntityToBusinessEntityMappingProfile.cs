using AutoMapper;
using ReMi.BusinessEntities;
using ReMi.BusinessEntities.Api;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.HelpDesk;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessEntities.Products;

namespace ReMi.BusinessLogic.AutoMapper
{
    public class BusinessEntityToBusinessEntityMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "BusinessEntityToBusinessEntityMapping"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<HelpDeskTask, HelpDeskTaskView>()
                .ForMember(x => x.Description, o => o.MapFrom(x => x.Subject))
                .ForMember(x => x.LinkUrl, o => o.MapFrom(x => x.Url));

            Mapper.CreateMap<ReleaseTaskAttachment, HelpDeskTicketAttachment>()
                .ForMember(x => x.FileName, o => o.MapFrom(x => x.Name))
                .ForMember(x => x.ReleaseTaskId, o => o.MapFrom(x => x.ReleaseTaskId))
                .ForMember(x => x.ReleaseAttachmentId, o => o.MapFrom(x => x.ExternalId))
                .ForMember(x => x.Data, o => o.MapFrom(x => x.Attachment));

            Mapper.CreateMap<JobMeasurement, MeasurementTime>()
                .ForMember(x => x.Name, o => o.MapFrom(x => x.StepName))
                .ForMember(x => x.Value, o => o.MapFrom(x => x.Duration / 1000 / 60));

            Mapper.CreateMap<ApiDescription, ApiDescriptionFull>();

            Mapper.CreateMap<Product, ProductView>()
                .ForMember(target => target.Name, option => option.MapFrom(source => source.Description))
                .ForMember(target => target.BusinessUnit, option => option.MapFrom(source => source.BusinessUnit != null ? source.BusinessUnit.Description : string.Empty));

            Mapper.CreateMap<BusinessUnit, BusinessUnitView>();
        }
    }
}
