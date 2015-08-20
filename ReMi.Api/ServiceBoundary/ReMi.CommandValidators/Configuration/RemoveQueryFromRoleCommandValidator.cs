using FluentValidation;
using ReMi.Commands.Configuration;
using ReMi.Common.Cqrs.FluentValidation;
using System;

namespace ReMi.CommandValidators.Configuration
{
    public class RemoveQueryFromRoleCommandValidator : RequestValidatorBase<RemoveQueryFromRoleCommand>
    {
        public RemoveQueryFromRoleCommandValidator()
        {
            RuleFor(x => x.QueryId).GreaterThan(0);
            RuleFor(x => x.RoleExternalId).NotEqual(Guid.Empty);
        }
    }
}
