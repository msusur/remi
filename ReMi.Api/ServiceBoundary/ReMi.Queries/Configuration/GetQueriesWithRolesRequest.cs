using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Configuration
{
    [Query("Get queries with permissions", QueryGroup.AccessControl)]
    public class GetQueriesWithRolesRequest : IQuery
    {
        public QueryContext Context { get; set; }
    }
}
