using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get release task types", QueryGroup.ReleasePlan, IsStatic = true)]
    public class GetReleaseTaskTypesRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return "[]";
        }
    }
}
