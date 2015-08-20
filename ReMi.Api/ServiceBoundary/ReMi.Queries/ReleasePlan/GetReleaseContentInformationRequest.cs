using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get release content", QueryGroup.ReleasePlan)]
    public class GetReleaseContentInformationRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}]", ReleaseWindowId);
        }
    }
}
