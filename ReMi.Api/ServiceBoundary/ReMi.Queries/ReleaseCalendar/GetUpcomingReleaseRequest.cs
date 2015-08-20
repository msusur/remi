using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleaseCalendar
{
    [Query("Get upcoming release details", QueryGroup.ReleasePlan)]
    public class GetUpcomingReleaseRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string Product { get; set; }

        public override string ToString()
        {
            return string.Format("[Product = {0}]", Product);
        }
    }
}
