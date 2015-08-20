using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get release approvers list", QueryGroup.ReleasePlan)]
    public class GetReleaseApproversRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindowId = {0}]", ReleaseWindowId);
        }
    }
}
