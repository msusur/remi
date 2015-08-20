using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryValidators.ReleaseCalendar
{
    public class GetUpcomingReleaseRequestValidator : RequestValidatorBase<GetUpcomingReleaseRequest>
    {
        public GetUpcomingReleaseRequestValidator()
        {
            RuleFor(request => request.Product).NotEmpty();
        }
    }
}
