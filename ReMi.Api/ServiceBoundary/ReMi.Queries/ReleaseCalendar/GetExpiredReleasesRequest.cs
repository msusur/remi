using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleaseCalendar
{
    [Query("Get expired releases", QueryGroup.ReleasePlan)]
    public class GetExpiredReleasesRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return string.Format("[Context = {0}]", Context);
        }
    }
}
