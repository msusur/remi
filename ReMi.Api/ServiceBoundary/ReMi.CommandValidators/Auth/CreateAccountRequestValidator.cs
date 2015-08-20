using System.Linq;
using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Auth
{
    public class CreateAccountRequestValidator : RequestValidatorBase<CreateAccountCommand>
    {
        public CreateAccountRequestValidator()
        {
            RuleFor(request => request.Account).NotNull();

            RuleFor(request => request.Account.ExternalId).NotEmpty();

            RuleFor(request => request.Account.Name).NotEmpty();

            RuleFor(request => request.Account.FullName).NotEmpty();

            RuleFor(request => request.Account.Email).NotEmpty();

            RuleFor(request => request.Account.Role).NotEmpty();

            RuleFor(request => request.Account.Products).Must(products => products.Any(o => o.IsDefault));
        }
    }
}
