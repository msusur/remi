using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.Auth;

namespace ReMi.QueryValidators.Auth
{
    public class PermissionsRequestValidator : RequestValidatorBase<PermissionsRequest>
    {
        public PermissionsRequestValidator()
        {
            RuleFor(x => x.RoleId).NotEmpty();
        }
    }
}
