using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Commands.ReleaseCalendar;

namespace ReMi.CommandValidators.ReleaseCalendar
{
    public class CancelReleaseWindowCommandValidator : RequestValidatorBase<CancelReleaseWindowCommand>
    {
        public CancelReleaseWindowCommandValidator()
        {
            RuleFor(request => request.ExternalId).NotNull().NotEmpty();
        }
    }
}
