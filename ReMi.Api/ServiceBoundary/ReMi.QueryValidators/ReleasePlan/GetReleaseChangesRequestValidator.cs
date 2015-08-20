using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryValidators.ReleasePlan
{
    public class GetReleaseChangesRequestValidator : RequestValidatorBase<GetReleaseChangesRequest>
    {
        public GetReleaseChangesRequestValidator()
        {
            RuleFor(x => x.ReleaseWindowId).NotEmpty();
        }
    }
}
