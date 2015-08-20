using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Auth
{
    public class UpdateAccountCommandValidator : RequestValidatorBase<UpdateAccountCommand>
    {
        public UpdateAccountCommandValidator()
        {
            RuleFor(request => request.Account).NotNull();

            RuleFor(request => request.Account.ExternalId).NotEmpty();

            RuleFor(request => request.Account.Name).NotEmpty();

            RuleFor(request => request.Account.FullName).NotEmpty();

            RuleFor(request => request.Account.Email).NotEmpty();

            RuleFor(request => request.Account.Role).NotEmpty();
        }
    }
}
