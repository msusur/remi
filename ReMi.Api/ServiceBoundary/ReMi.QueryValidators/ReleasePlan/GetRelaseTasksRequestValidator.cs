using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryValidators.ReleasePlan
{
    public class GetRelaseTasksRequestValidator : RequestValidatorBase<GetReleaseTasksRequest>
    {
        public GetRelaseTasksRequestValidator()
        {
            RuleFor(request => request.ReleaseWindowId).NotEmpty();
        }
    }
}
