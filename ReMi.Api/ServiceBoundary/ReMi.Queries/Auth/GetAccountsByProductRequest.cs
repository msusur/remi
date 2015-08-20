using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Auth
{
    [Query("Get accounts by package", QueryGroup.AccessControl)]
    public class GetAccountsByProductRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string Product { get; set; }

        public override string ToString()
        {
            return string.Format("[Product={0}]", Product);
        }
    }
}
