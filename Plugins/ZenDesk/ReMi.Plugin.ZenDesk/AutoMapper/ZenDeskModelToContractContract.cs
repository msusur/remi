using AutoMapper;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using ReMi.Plugin.ZenDesk.Models;

namespace ReMi.Plugin.ZenDesk.AutoMapper
{
    public class ZenDeskModelToContractContract : Profile
    {
        public override string ProfileName
        {
            get { return "ZenDeskModelToContractContract"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<Ticket, HelpDeskTicket>()
                .ForMember(target => target.Description, option => option.MapFrom(source => source.Description))
                .ForMember(target => target.Subject, option => option.MapFrom(source => source.Subject))
                .ForMember(target => target.Id, option => option.MapFrom(source => source.Id.ToString()))
                .ForMember(target => target.Url, option => option.MapFrom(source => source.TicketUrl))
                .ForMember(target => target.Comment, option => option.MapFrom(source => source.Comment == null ? null : source.Comment.Body));
        }
    }
}
