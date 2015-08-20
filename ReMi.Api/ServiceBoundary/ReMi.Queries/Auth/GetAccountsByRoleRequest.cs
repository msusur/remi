using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Auth
{
    [Query("Get account by role", QueryGroup.AccessControl)]
    public class GetAccountsByRoleRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string Role { get; set; }

        public override string ToString()
        {
            return string.Format("[]");
        }
    }
}
