using AutoMapper;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Plugin.Email.Models;

namespace ReMi.Plugin.Email.AutoMapper
{
    public class EmailBusinessEnitiesToContractEntities : Profile
    {
        public override string ProfileName
        {
            get { return "EmailBusinessEnitiesToContractEntities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<OutlookEmailMethod, CalendarEventType>();
            Mapper.CreateMap<OutlookEventEntity, CalendarEvent>()
                .ForMember(target => target.CalendarEventType, opt => opt.MapFrom(source => source.OutlookEmailMethod));
        }
    }
}
