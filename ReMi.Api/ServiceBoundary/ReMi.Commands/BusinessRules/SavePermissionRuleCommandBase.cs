
using ReMi.BusinessEntities.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.BusinessRules
{
    public abstract class SavePermissionRuleCommandBase : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public BusinessRuleDescription Rule { get; set; }
    }
}
