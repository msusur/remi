using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryValidators.ReleaseCalendar
{
    public class GetCurrentReleaseRequestValidator : RequestValidatorBase<GetCurrentReleaseRequest>
    {
        public GetCurrentReleaseRequestValidator()
        {
            RuleFor(request => request.Product).NotEmpty();
        }
    }
}
