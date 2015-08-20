using System;
using FluentValidation;
using ReMi.Commands.BusinessRules;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.BusinessRules
{
    public class DeletePermissionRuleCommandValidator : RequestValidatorBase<DeletePermissionRuleCommand>
    {
        public DeletePermissionRuleCommandValidator()
        {
            RuleFor(x => x.RuleId).Must(x => x != Guid.Empty)
                .WithMessage("RuleId cannot be empty");
        }
    }
}
