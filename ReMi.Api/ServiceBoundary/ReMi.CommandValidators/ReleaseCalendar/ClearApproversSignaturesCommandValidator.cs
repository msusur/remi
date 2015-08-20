using FluentValidation;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleaseCalendar
{
    public class ClearApproversSignaturesCommandValidator : RequestValidatorBase<ClearApproversSignaturesCommand>
    {
        public ClearApproversSignaturesCommandValidator()
        {
            RuleFor(x => x.ReleaseWindowId).NotEmpty();
        }
    }
}
