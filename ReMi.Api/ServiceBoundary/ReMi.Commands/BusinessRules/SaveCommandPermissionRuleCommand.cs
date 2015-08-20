using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.BusinessRules
{
    [Command("Save command permission rule", CommandGroup.BusinessRules)]
    public class SaveCommandPermissionRuleCommand : SavePermissionRuleCommandBase
    {
        public int CommandId { get; set; }

        public override string ToString()
        {
            return String.Format("[Rule={0}, CommandId={1}, CommandContext={2}]", Rule, CommandId, CommandContext);
        }
    }
}
