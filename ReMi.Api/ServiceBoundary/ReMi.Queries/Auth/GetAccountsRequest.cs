using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Auth
{
    [Query("Get accounts", QueryGroup.AccessControl)]
    public class GetAccountsRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return string.Format("[]");
        }
    }
}
