using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get release task details", QueryGroup.ReleasePlan)]
    public class GetReleaseTaskRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid ReleaseTaskId { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseTaskId={0}]", ReleaseTaskId);
        }
    }
}
