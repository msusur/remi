using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get release tasks", QueryGroup.ReleasePlan)]
    public class GetReleaseTasksRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindowId = {0}]", ReleaseWindowId);
        }
    }
}
