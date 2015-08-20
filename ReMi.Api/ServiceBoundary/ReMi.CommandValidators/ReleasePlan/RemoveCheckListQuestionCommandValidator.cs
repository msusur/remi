using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class RemoveCheckListQuestionCommandValidator : RequestValidatorBase<RemoveCheckListQuestionCommand>
    {
        public RemoveCheckListQuestionCommandValidator()
        {
            RuleFor(x => x.CheckListQuestionId).NotEmpty();
        }
    }
}
