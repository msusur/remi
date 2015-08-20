using FluentValidation;
using ReMi.Commands.BusinessRules;
using ReMi.Common.Cqrs.FluentValidation;
using System;
using System.Linq;

namespace ReMi.CommandValidators.BusinessRules
{
    public abstract class SavePermissionRuleCommandBaseValidator<T> : RequestValidatorBase<T>
        where T : SavePermissionRuleCommandBase
    {
        protected SavePermissionRuleCommandBaseValidator()
        {
            RuleFor(x => x.Rule).NotNull();
            RuleFor(x => x.Rule.Description).NotEmpty();
            RuleFor(x => x.Rule.ExternalId).NotEqual(Guid.Empty);
            RuleFor(x => x.Rule.Name).NotEmpty();
            RuleFor(x => x.Rule.Script).NotEmpty();
            RuleFor(x => x.Rule.AccountTestData).NotNull();
            RuleFor(x => x.Rule.AccountTestData.ExternalId).NotEqual(Guid.Empty);
            RuleFor(x => x.Rule.AccountTestData.JsonData).NotEmpty();
            RuleFor(x => x.Rule.Parameters)
                .Must(x => x != null && x.Count() == 1)
                .WithMessage("Parameters count for command has to be exact 1");
            RuleFor(x => x.Rule.Parameters).SetCollectionValidator(new BusinessRuleParameterValidator());
        }
    }
}
