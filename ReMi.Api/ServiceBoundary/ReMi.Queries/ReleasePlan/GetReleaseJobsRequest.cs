using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get deployment jobs for release", QueryGroup.ReleasePlan)]
    public class GetReleaseJobsRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, Context={1}]", 
                ReleaseWindowId, Context);
        }
    }
}
