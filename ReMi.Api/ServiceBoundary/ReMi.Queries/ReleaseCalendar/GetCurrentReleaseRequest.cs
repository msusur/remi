using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleaseCalendar
{
    [Query("Get current release details", QueryGroup.ReleaseCalendar)]
    public class GetCurrentReleaseRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string Product { get; set; }

        public override string ToString()
        {
            return string.Format("[Product = {0}, Context={1}]",
                Product, Context);
        }
    }
}
