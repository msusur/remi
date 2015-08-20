using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Auth
{
    public class UpdateRoleCommandValidator : RequestValidatorBase<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(request => request.Role).NotNull();

            RuleFor(request => request.Role.ExternalId).NotEmpty();

            RuleFor(request => request.Role.Name).NotEmpty();

            RuleFor(request => request.Role.Description).NotEmpty();
        }
    }
}
