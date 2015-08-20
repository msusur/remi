using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryValidators.ReleasePlan
{
    public class CheckListAdditionalQuestionRequestValidator : RequestValidatorBase<CheckListAdditionalQuestionRequest>
    {
        public CheckListAdditionalQuestionRequestValidator()
        {
            RuleFor(request => request.ReleaseWindowId).NotEmpty();
        }
    }
}
