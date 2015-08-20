using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleaseParticipant
{
    [Query("Get paraticipants", QueryGroup.ReleasePlan)]
    public class GetReleaseParticipantRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("ReleaseWindowId={0}", ReleaseWindowId);
        }
    }
}
