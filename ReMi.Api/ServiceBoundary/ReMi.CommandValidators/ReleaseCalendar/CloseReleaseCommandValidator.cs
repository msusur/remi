using FluentValidation;
using ReMi.BusinessEntities.Auth;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleaseCalendar
{
    public class CloseReleaseCommandValidator : RequestValidatorBase<CloseReleaseCommand>
    {
        public CloseReleaseCommandValidator()
        {
            RuleFor(o => o.ReleaseWindowId).NotEmpty();

            RuleFor(o => o.Recipients).SetCollectionValidator(new AccountValidator());

            RuleFor(o => o.UserName).NotEmpty();

            RuleFor(o => o.Password).NotEmpty();
        }

        private  class AccountValidator : AbstractValidator<Account> 
        {
            public AccountValidator()
            {
                RuleFor(x => x.ExternalId).NotEmpty();
                RuleFor(x => x.Email).EmailAddress().NotEmpty();
            }
        }
    }
}
