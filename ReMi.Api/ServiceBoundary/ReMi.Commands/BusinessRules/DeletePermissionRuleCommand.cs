using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.BusinessRules
{
    [Command("Delete permission rule", CommandGroup.BusinessRules)]
    public class DeletePermissionRuleCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid RuleId { get; set; }

        public override string ToString()
        {
            return String.Format("[RuleId={0}, CommandContext={1}]", RuleId, CommandContext);
        }
    }
}
