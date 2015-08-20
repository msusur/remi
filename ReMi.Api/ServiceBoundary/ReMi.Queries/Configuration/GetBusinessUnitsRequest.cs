using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Configuration
{
    [Query("Get business unites", QueryGroup.Configuration)]
    public class GetBusinessUnitsRequest : IQuery
    {
        public bool IncludeAll { get; set; }

        public override string ToString()
        {
            return "[]";
        }

        public QueryContext Context { get; set; }
    }
}
