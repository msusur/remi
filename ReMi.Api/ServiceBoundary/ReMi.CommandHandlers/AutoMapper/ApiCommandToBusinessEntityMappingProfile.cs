using AutoMapper;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Commands.Configuration;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Commands.ReleasePlan;
using BusinessReleaseWindow = ReMi.BusinessEntities.ReleaseCalendar.ReleaseWindow;
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;

namespace ReMi.CommandHandlers.AutoMapper
{
    public class ApiCommandToBusinessEntityMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ApiCommandToBusinessEntityMapping"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<BookReleaseWindowCommand, BusinessReleaseWindow>()
                .ConvertUsing<BasicReleaseWindowRequestToReleaseWindowConverter>();
            //DOES NOT WORK: .ForMember(target => target, option => option.MapFrom(source => source.ReleaseWindow))

            Mapper.CreateMap<CancelReleaseWindowCommand, BusinessReleaseWindow>();

            Mapper.CreateMap<UpdateReleaseWindowCommand, BusinessReleaseWindow>()
                .ConvertUsing<BasicReleaseWindowRequestToReleaseWindowConverter>();

            Mapper.CreateMap<UpdateTicketRiskCommand, ReleaseContentTicket>()
                .ForMember(target => target.LastChangedByAccount, option => option.MapFrom(source => source.CommandContext.UserId))
                .ForMember(target => target.TicketName, option => option.MapFrom(source => source.TicketKey));
            Mapper.CreateMap<UpdateTicketCommentCommand, ReleaseContentTicket>()
                .ForMember(target => target.LastChangedByAccount, option => option.MapFrom(source => source.CommandContext.UserId))
                .ForMember(target => target.TicketName, option => option.MapFrom(source => source.TicketKey));

            Mapper.CreateMap<AddProductCommand, Product>()
                .ForMember(target => target.BusinessUnit, option => option.MapFrom(source => new BusinessUnit { ExternalId = source.BusinessUnitId }));
            Mapper.CreateMap<UpdateProductCommand, Product>()
                .ForMember(target => target.BusinessUnit, option => option.MapFrom(source => new BusinessUnit { ExternalId = source.BusinessUnitId }));
        }
    }
}
