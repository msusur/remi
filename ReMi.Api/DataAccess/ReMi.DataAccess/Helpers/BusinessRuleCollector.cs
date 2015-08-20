using System.Linq;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils;
using ReMi.DataEntities.BusinessRules;
using System.Collections.Generic;

namespace ReMi.DataAccess.Helpers
{
    public static class BusinessRuleCollector
    {
        public static IEnumerable<BusinessRuleDescription> Collect()
        {
            var result = new List<BusinessRuleDescription>();

            var businessRuleType = typeof (BusinessRuleConstants);
            var ruleConstantsType = typeof (RuleConstants);
            var groupInterface = typeof (IGroupConstants);

            foreach (var groupField in businessRuleType.GetFields()
                .Where(x => x.IsStatic && groupInterface.IsAssignableFrom(x.FieldType)))
            {
                var group = (IGroupConstants) groupField.GetValue(null);
                foreach (var ruleField in groupField.FieldType.GetFields()
                    .Where(x => ruleConstantsType.IsAssignableFrom(x.FieldType)))
                {
                    var rule = (RuleConstants) ruleField.GetValue(group);
                    var ruleDescription = new BusinessRuleDescription
                    {
                        Description = rule.Description,
                        Name = rule.Name,
                        ExternalId = rule.ExternalId,
                        Group = group.Group
                    };
                    ruleDescription.Parameters = rule.Parameters.IsNullOrEmpty()
                        ? null
                        : rule.Parameters.Select(x => new BusinessRuleParameter
                        {
                            ExternalId = x.ExternalId,
                            Name = x.Name,
                            Type = x.Type,
                            BusinessRule = ruleDescription
                        }).ToList();
                    result.Add(ruleDescription);
                }
            }

            return result;
        }
    }
}
