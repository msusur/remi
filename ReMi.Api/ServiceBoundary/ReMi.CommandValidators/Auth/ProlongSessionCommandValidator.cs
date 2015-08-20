using System;
using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Auth
{
    public class ProlongSessionCommandValidator : RequestValidatorBase<ProlongSessionCommand>
    {
        public ProlongSessionCommandValidator()
        {
            RuleFor(x => x.SessionId).Must(x => x != Guid.Empty).WithMessage("Session Id cannot be empty");
        }
    }
}
