using ReMi.BusinessEntities.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.BusinessRules
{
    [Query("Test Business Rule", QueryGroup.BusinessRules)]
    public class TestBusinessRuleRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public BusinessRuleDescription Rule { get; set; }

        public override string ToString()
        {
            return string.Format("[Rule={0}]", Rule);
        }
    }
}
