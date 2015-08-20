using FluentValidation;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleaseCalendar
{
    public class FailReleaseCommandValidator : RequestValidatorBase<FailReleaseCommand>
    {
        public FailReleaseCommandValidator()
        {
            RuleFor(o => o.ReleaseWindowId).NotEmpty();

            RuleFor(o => o.UserName).NotEmpty();

            RuleFor(o => o.Password).NotEmpty();
        }
    }
}
