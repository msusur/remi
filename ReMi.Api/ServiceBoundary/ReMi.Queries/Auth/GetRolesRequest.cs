using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Auth
{
    [Query("Get roles", QueryGroup.AccessControl)]
    public class GetRolesRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return "[]";
        }
    }
}
