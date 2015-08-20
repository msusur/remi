using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.CommandValidators.Common;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Common.Utils;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class AddCheckListQuestionsCommandValidator : RequestValidatorBase<AddCheckListQuestionsCommand>
    {
        public AddCheckListQuestionsCommandValidator()
        {
            RuleFor(x => x.ReleaseWindowId).NotEmpty();
            RuleFor(request => request.QuestionsToAdd).SetCollectionValidator(new QuestionValidator());
            RuleFor(request => request.QuestionsToAssign).SetCollectionValidator(new QuestionValidator());
            When(x => x.QuestionsToAssign.IsNullOrEmpty(), () =>
                RuleFor(x => x.QuestionsToAdd).NotEmpty());
            When(x => x.QuestionsToAdd.IsNullOrEmpty(), () =>
                RuleFor(x => x.QuestionsToAssign).NotEmpty());
        }
    } 
}
