using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Auth
{
    public class DeleteRoleCommandValidator : RequestValidatorBase<DeleteRoleCommand>
    {
        public DeleteRoleCommandValidator()
        {
            RuleFor(request => request.Role).NotNull();

            RuleFor(request => request.Role.ExternalId).NotEmpty();
        }
    }
}
