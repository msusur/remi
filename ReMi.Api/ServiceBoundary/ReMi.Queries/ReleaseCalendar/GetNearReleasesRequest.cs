using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleaseCalendar
{
    [Query("Get near releases details", QueryGroup.ReleasePlan)]
    public class GetNearReleasesRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string Product { get; set; }

        public override string ToString()
        {
            return string.Format("[Product = {0}]", Product);
        }
    }
}
