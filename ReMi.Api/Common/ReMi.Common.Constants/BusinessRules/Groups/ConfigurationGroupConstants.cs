using System;

namespace ReMi.Common.Constants.BusinessRules.Groups
{
    public class ConfigurationGroupConstants : IGroupConstants
    {
        public BusinessRuleGroup Group { get { return BusinessRuleGroup.Configuration; } }

        public readonly RuleConstants SessionDurationRule = new RuleConstants
        {
            Name = "SessionDurationRule",
            Description = "Session Duration Rule",
            ExternalId = Guid.Parse("2712503c-3c83-421b-9c08-38fe66ed201a")
        };
    }
}
