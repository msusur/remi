
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Configuration
{
    [Query("Get packages", QueryGroup.Configuration)]
    public class GetProductsRequest : IQuery
    {
        public override string ToString()
        {
            return "[]";
        }

        public QueryContext Context { get; set; }
    }
}
