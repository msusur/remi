using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryValidators.ReleaseCalendar
{
    public class GetNearReleasesRequestValidator : RequestValidatorBase<GetNearReleasesRequest>
    {
        public GetNearReleasesRequestValidator()
        {
            RuleFor(request => request.Product).NotEmpty();
        }
    }
}
