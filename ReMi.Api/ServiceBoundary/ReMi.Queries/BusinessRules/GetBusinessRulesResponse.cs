using System.Collections.Generic;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils;

namespace ReMi.Queries.BusinessRules
{
    public class GetBusinessRulesResponse
    {
        public IDictionary<BusinessRuleGroup, IEnumerable<BusinessRuleView>> Rules { get; set; }

        public override string ToString()
        {
            return string.Format("[Rules={0}]",
                Rules.FormatElements());
        }
    }
}
