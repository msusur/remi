using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get checklist", QueryGroup.ReleasePlan)]
    public class GetCheckListRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindowId = {0}]", ReleaseWindowId);
        }
    }
}
