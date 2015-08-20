using AutoMapper;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using ReMi.Plugin.ZenDesk.Models;

namespace ReMi.Plugin.ZenDesk.AutoMapper
{
    public class ContractModelToZenDeskModel : Profile
    {
        public override string ProfileName
        {
            get { return "ContractModelToZenDeskModel"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<HelpDeskTicket, Ticket>()
                .ForMember(target => target.Comment, option => option.MapFrom(source => new TicketComment { Body = source.Comment }))
                .ForMember(target => target.Description, option => option.MapFrom(source => source.Description))
                .ForMember(target => target.Subject, option => option.MapFrom(source => source.Subject))
                .ForMember(target => target.Type, option => option.UseValue(Types.task))
                .ForMember(target => target.Id, option => option.MapFrom(source => string.IsNullOrEmpty(source.Id) ? -1 : int.Parse(source.Id)));
        }
    }
}
