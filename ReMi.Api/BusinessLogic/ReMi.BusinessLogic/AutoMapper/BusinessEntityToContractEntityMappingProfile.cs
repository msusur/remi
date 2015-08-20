using AutoMapper;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Plugins.Data.HelpDesk;

namespace ReMi.BusinessLogic.AutoMapper
{
    public class BusinessEntityToContractEntityMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "BusinessEntityToContractEntityMappingProfile"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<ReleaseContentTicket, Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket>();

            Mapper.CreateMap<ReleaseTask, HelpDeskTicket>()
                .ForMember(x => x.Id, o => o.MapFrom(x => x.HelpDeskTicketReference))
                .ForMember(x => x.Comment, o => o.MapFrom(x => x.Description))
                .ForMember(x => x.Subject, o => o.Ignore());
        }
    }
}
