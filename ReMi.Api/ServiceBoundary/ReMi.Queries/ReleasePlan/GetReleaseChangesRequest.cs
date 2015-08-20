using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get release source control changes", QueryGroup.ReleasePlan)]
    public class GetReleaseChangesRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid ReleaseWindowId { get; set; }
        public Boolean IsBackground { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, IsBackground={1}, Context={2}]", ReleaseWindowId, IsBackground,
                Context);
        }
    }
}
