using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleaseCalendar
{
    [Query("Get release types", QueryGroup.ReleaseCalendar, IsStatic = true)]
    public class GetReleaseEnumsRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return "[]";
        }
    }
}
