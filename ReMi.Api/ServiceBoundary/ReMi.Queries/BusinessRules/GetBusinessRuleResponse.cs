
using ReMi.BusinessEntities.BusinessRules;

namespace ReMi.Queries.BusinessRules
{
    public class GetBusinessRuleResponse
    {
        public BusinessRuleDescription Rule { get; set; }

        public override string ToString()
        {
            return string.Format("[Rule={0}]",
                Rule);
        }
    }
}
