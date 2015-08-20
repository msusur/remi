using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryValidators.ReleaseCalendar
{
    public class GetReleaseRequestValidator : RequestValidatorBase<GetReleaseRequest>
    {
        public GetReleaseRequestValidator()
        {
            RuleFor(o => o.ReleaseWindowId).NotEmpty();
        }
    }
}
