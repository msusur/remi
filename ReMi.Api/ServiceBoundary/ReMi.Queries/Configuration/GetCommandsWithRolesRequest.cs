using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Configuration
{
    [Query("Get Commands with permissions", QueryGroup.AccessControl)]
    public class GetCommandsWithRolesRequest : IQuery
    {
        public QueryContext Context { get; set; }
    }
}
