using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryValidators.ReleasePlan
{
    public class GetReleaseApproversRequestValidator : RequestValidatorBase<GetReleaseApproversRequest>
    {
        public GetReleaseApproversRequestValidator()
        {
            RuleFor(request => request.ReleaseWindowId).NotEmpty();
        }
    }
}
