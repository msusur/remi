using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryValidators.ReleaseCalendar
{
    public class GetReleaseCalendarRequestValidator : RequestValidatorBase<GetReleaseCalendarRequest>
    {
        public GetReleaseCalendarRequestValidator()
        {
            RuleFor(request => request.EndDay)
                .Must((instance, endDay) => endDay > instance.StartDay)
                .WithMessage("EndDay ({0:yyyy-MM-dd}) must be posterior to StartDay ({1:yyyy-MM-dd})",
                    request => request.EndDay, request => request.StartDay);
        }
    }
}
