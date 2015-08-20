using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Auth
{
    public class StartSessionCommandValidator : RequestValidatorBase<StartSessionCommand>
    {
        public StartSessionCommandValidator()
        {
            RuleFor(x => x.SessionId).NotEmpty();
            RuleFor(x => x.Login).NotEmpty();
        }
    }
}
