using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Auth
{
    public class AssociateAccountsWithProductRequestValidator : RequestValidatorBase<AssociateAccountsWithProductCommand>
    {
        public AssociateAccountsWithProductRequestValidator()
        {
            RuleFor(request => request.Accounts).NotNull().NotEmpty();

            RuleFor(request => request.ReleaseWindowId).NotEmpty();}
        }
}
