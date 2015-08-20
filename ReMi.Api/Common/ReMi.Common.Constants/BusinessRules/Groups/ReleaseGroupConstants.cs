using System;

namespace ReMi.Common.Constants.BusinessRules.Groups
{
    public class ReleaseGroupConstants : IGroupConstants
    {
        public BusinessRuleGroup Group { get { return BusinessRuleGroup.Release; } }

        public readonly RuleConstants ReleaseApprovalRule = new RuleConstants
        {
            Name = "ReleaseApprovalRule",
            Description = "Release approval rule",
            ExternalId = Guid.Parse("C28A1E71-FF85-4129-A083-821C6FF10BF3"),
            Parameters = new[]
            {
                new ParameterConstants
                {
                    ExternalId = Guid.Parse("E74253A5-CFDA-4B78-99A6-CD2A241742FB"),
                    Name = "approvers",
                    Type = "System.Collections.Generic.IEnumerable<ReMi.BusinessEntities.ReleasePlan.ReleaseApprover>"
                },
                new ParameterConstants
                {
                    ExternalId = Guid.Parse("CE9F280C-6DB5-4265-9B4B-7423581080C6"),
                    Name = "releaseWindow",
                    Type = "ReMi.BusinessEntities.ReleaseCalendar.ReleaseWindow"
                }
            }
        };

        public readonly RuleConstants AllowCloseAfterSignOffRule = new RuleConstants
        {
            Name = "AllowCloseAfterSignOffRule",
            Description = "Allow close after SignOff release",
            ExternalId = Guid.Parse("ABE45431-11FA-442C-8EFD-4BD75CC65451"),
            Parameters = new[]
            {
                new ParameterConstants
                {
                    ExternalId = Guid.Parse("CAECA2F5-DD93-4CF9-BD93-D3E7DDEC63C5"),
                    Name = "signOffs",
                    Type = "System.Collections.Generic.IEnumerable<ReMi.BusinessEntities.ReleaseExecution.SignOff>"
                },
                new ParameterConstants
                {
                    ExternalId = Guid.Parse("354369E2-7264-4E31-8774-3E107C32761A"),
                    Name = "releaseWindow",
                    Type = "ReMi.BusinessEntities.ReleaseCalendar.ReleaseWindow"
                }
            }
        };
    }
}
