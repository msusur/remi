using AutoMapper;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Plugin.Email.Models;

namespace ReMi.Plugin.Email.AutoMapper
{
    public class EmailContractEntitiesToBusinessEnities : Profile
    {
        public override string ProfileName
        {
            get { return "EmailContractEntitiesToBusinessEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<CalendarEventType, OutlookEmailMethod>();
            Mapper.CreateMap<CalendarEvent, OutlookEventEntity>()
                .ForMember(target => target.OutlookEmailMethod, opt => opt.MapFrom(source => source.CalendarEventType));
        }
    }
}
