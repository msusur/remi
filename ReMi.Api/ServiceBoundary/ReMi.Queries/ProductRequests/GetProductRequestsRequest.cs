using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ProductRequests
{
    [Query("Get product request config", QueryGroup.Configuration)]
    public class GetProductRequestsRequest : IQuery
    {
        public override string ToString()
        {
            return string.Format("[Context={0}]", Context);
        }

        public QueryContext Context { get; set; }
    }
}
