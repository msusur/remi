using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Constants;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.Configuration;

namespace ReMi.QueryHandlers.Configuration
{
    public class GetReleaseTracksHandler : IHandleQuery<GetReleaseTrackRequest, GetReleaseTrackResponse>
    {
        public GetReleaseTrackResponse Handle(GetReleaseTrackRequest request)
        {
            return new GetReleaseTrackResponse
            {
                ReleaseTrack = EnumDescriptionHelper.GetEnumDescriptions<ReleaseTrack, ReleaseTrackDescription>()
            };
        }
    }
}
