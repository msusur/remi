using FluentValidation;
using ReMi.Commands.Configuration;
using ReMi.Common.Cqrs.FluentValidation;
using System;

namespace ReMi.CommandValidators.Configuration
{
    public class RemoveCommandFromRoleCommandValidator : RequestValidatorBase<RemoveCommandFromRoleCommand>
    {
        public RemoveCommandFromRoleCommandValidator()
        {
            RuleFor(x => x.CommandId).GreaterThan(0);
            RuleFor(x => x.RoleExternalId).NotEqual(Guid.Empty);
        }
    }
}
