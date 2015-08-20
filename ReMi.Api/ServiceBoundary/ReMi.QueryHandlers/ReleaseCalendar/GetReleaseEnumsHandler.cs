using System.Linq;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Constants;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryHandlers.ReleaseCalendar
{
    public class GetReleaseEnumsHandler : IHandleQuery<GetReleaseEnumsRequest, GetReleaseEnumsResponse>
    {
        public GetReleaseEnumsResponse Handle(GetReleaseEnumsRequest request)
        {
            var types = EnumDescriptionHelper.GetEnumDescriptions<ReleaseType, ReleaseTypeDescription>().ToArray();

            return new GetReleaseEnumsResponse
            {
                ReleaseTypes =  types
            };
        }
    }
}
