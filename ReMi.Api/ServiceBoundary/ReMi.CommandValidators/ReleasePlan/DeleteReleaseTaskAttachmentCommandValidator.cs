using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class DeleteReleaseTaskAttachmentCommandValidator : RequestValidatorBase<DeleteReleaseTaskAttachmentCommand>
    {
        public DeleteReleaseTaskAttachmentCommandValidator()
        {
            RuleFor(x => x.ReleaseTaskAttachmentId).NotEmpty();
        }
    }
}
