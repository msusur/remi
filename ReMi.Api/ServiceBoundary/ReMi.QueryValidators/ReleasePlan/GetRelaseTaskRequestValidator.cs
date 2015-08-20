using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryValidators.ReleasePlan
{
    public class GetRelaseTaskRequestValidator : RequestValidatorBase<GetReleaseTaskRequest>
    {
        public GetRelaseTaskRequestValidator()
        {
            RuleFor(request => request.ReleaseTaskId).NotEmpty();
        }
    }
}
