using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Auth
{
    public class CreateRoleCommandValidator : RequestValidatorBase<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(request => request.Role).NotNull();

            RuleFor(request => request.Role.ExternalId).NotEmpty();

            RuleFor(request => request.Role.Name).NotEmpty();

            RuleFor(request => request.Role.Description).NotEmpty();
        }
    }
}
