using FluentValidation;
using ReMi.BusinessEntities.ReleasePlan;

namespace ReMi.CommandValidators.Common
{
    public class QuestionValidator : AbstractValidator<CheckListQuestion>
    {
        public QuestionValidator()
        {
            RuleFor(q => q.Question).NotEmpty();
            RuleFor(q => q.ExternalId).NotEmpty();
            RuleFor(q => q.CheckListId).NotEmpty();
        }
    }
}
