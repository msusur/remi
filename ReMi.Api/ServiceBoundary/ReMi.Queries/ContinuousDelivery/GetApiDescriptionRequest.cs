using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ContinuousDelivery
{
    [Query("Get api description", QueryGroup.ContinuousDelivery)]
    public class GetApiDescriptionRequest : IQuery
    {
        public QueryContext Context { get; set; }
    }
}
