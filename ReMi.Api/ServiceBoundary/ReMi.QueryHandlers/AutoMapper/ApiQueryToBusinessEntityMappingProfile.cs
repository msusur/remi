using AutoMapper;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryHandlers.AutoMapper
{
    public class ApiQueryToBusinessEntityMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ApiQueryToBusinessEntityMapping"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<GetReleaseCalendarRequest, ReleaseCalendarFilter>()
                .ForMember(target => target.StartDay, option => option.MapFrom(source => source.StartDay.ToUniversalTime()))
                .ForMember(target => target.EndDay, option => option.MapFrom(source => source.EndDay.ToUniversalTime()));
        }
    }
}
