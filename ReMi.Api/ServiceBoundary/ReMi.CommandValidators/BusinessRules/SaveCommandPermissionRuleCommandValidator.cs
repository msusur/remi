using FluentValidation;
using ReMi.Commands.BusinessRules;

namespace ReMi.CommandValidators.BusinessRules
{
    public class SaveCommandPermissionRuleCommandValidator : SavePermissionRuleCommandBaseValidator<SaveCommandPermissionRuleCommand>
    {
        public SaveCommandPermissionRuleCommandValidator()
        {
            RuleFor(x => x.CommandId).GreaterThan(0);
        }
    }
}
