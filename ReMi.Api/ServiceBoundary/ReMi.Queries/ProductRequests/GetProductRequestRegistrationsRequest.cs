using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ProductRequests
{
    [Query("Get product request registrations", QueryGroup.ProductRequests)]
    public class GetProductRequestRegistrationsRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return "[]";
        }
    }
}
