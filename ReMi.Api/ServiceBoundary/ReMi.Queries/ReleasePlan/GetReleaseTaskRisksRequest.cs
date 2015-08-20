using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get release task risk", QueryGroup.ReleasePlan, IsStatic = true)]
    public class GetReleaseTaskRisksRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return "[]";
        }
    }
}
