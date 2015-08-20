using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get ticket risk list", QueryGroup.ReleasePlan, IsStatic = true)]
    public class GetTicketRiskRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return "[]";
        }
    }
}
