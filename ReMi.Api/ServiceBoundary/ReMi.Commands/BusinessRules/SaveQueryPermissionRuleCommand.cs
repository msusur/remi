using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.BusinessRules
{
    [Command("Save query permission rule", CommandGroup.BusinessRules)]
    public class SaveQueryPermissionRuleCommand : SavePermissionRuleCommandBase
    {
        public int QueryId { get; set; }

        public override string ToString()
        {
            return String.Format("[Rule={0}, QueryId={1}, CommandContext={2}]", Rule, QueryId, CommandContext);
        }
    }
}
