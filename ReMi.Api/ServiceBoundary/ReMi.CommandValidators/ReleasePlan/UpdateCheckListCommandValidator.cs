using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class UpdateCheckListCommandValidator : RequestValidatorBase<UpdateCheckListCommentCommand>
    {
        public UpdateCheckListCommandValidator()
        {
            RuleFor(x => x.CheckListItem).NotNull();

            RuleFor(x => x.CheckListItem.Comment).Length(0, 4000);
        }
    }
}
