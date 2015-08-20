using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Metrics
{
    [Query("Get metrics for specified release", QueryGroup.Metrics)]
    public class GetMetricsRequest : IQuery
    {
        public QueryContext Context { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, Context={1}]", ReleaseWindowId, Context);
        }
    }
}
