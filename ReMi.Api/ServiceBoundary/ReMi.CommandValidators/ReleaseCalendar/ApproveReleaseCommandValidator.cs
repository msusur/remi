using FluentValidation;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleaseCalendar
{
    public class ApproveReleaseCommandValidator : RequestValidatorBase<ApproveReleaseCommand>
    {
        public ApproveReleaseCommandValidator()
        {
            RuleFor(o => o.ReleaseWindowId).NotEmpty();

            RuleFor(o => o.AccountId).NotEmpty();

            RuleFor(o => o.UserName).NotEmpty();

            RuleFor(o => o.Password).NotEmpty();
        }
    }
}
