using System;
using FluentValidation;
using ReMi.Commands.ReleaseExecution;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleaseExecution
{
    public class SignOffReleaseCommandValidator : RequestValidatorBase<SignOffReleaseCommand>
    {
        public SignOffReleaseCommandValidator()
        {
            RuleFor(x => x.ReleaseWindowId).Must(x => x != Guid.Empty);

            RuleFor(o => o.AccountId).NotEmpty();

            RuleFor(o => o.UserName).NotEmpty();

            RuleFor(o => o.Password).NotEmpty();
        }
    }
}
