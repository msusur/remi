using FluentValidation;
using ReMi.Commands.BusinessRules;

namespace ReMi.CommandValidators.BusinessRules
{
    public class SaveQueryPermissionRuleCommandValidator : SavePermissionRuleCommandBaseValidator<SaveQueryPermissionRuleCommand>
    {
        public SaveQueryPermissionRuleCommandValidator()
        {
            RuleFor(x => x.QueryId).GreaterThan(0);
        }
    }
}
