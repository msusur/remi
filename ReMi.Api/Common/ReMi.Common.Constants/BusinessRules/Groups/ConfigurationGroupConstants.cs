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

        public readonly RuleConstants TeamRoleRule = new RuleConstants
        {
            Name = "TeamRoleRule",
            Description = "Team Role Rule",
            ExternalId = Guid.Parse("2b1c3b65-2f42-46ab-d7ea-dfe6543a4a85"),
            Parameters = new[]
            {
                new ParameterConstants
                {
                    ExternalId = Guid.Parse("7a520fbc-4c6f-4360-ef9b-65761e9fbe3c"),
                    Name = "roleName",
                    Type = "System.String"
                }
            }
        };
    }
}
