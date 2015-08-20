using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Configuration
{
    [Query("Get Commands by names with permissions", QueryGroup.AccessControl)]
    public class GetCommandsByNamesRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string Names { get; set; }
    }
}
