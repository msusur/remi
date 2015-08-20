using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get checklist additional questions", QueryGroup.ReleasePlan)]
    public class CheckListAdditionalQuestionRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}]", ReleaseWindowId);
        }
    }
}
