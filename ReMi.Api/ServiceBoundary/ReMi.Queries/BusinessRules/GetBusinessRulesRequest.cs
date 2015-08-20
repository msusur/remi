using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.BusinessRules
{
    [Query("Get Business Rules", QueryGroup.BusinessRules)]
    public class GetBusinessRulesRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return string.Format("[Context={0}]", Context);
        }
    }
}
