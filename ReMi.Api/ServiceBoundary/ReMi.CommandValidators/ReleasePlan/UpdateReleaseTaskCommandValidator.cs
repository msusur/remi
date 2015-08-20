using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class UpdateReleaseTaskCommandValidator : RequestValidatorBase<UpdateReleaseTaskCommand>
    {
        public UpdateReleaseTaskCommandValidator()
        {
            RuleFor(x => x.ReleaseTask).NotNull();
            RuleFor(x => x.ReleaseTask.ExternalId).NotEmpty();
            RuleFor(x => x.ReleaseTask.Description)
                .NotEmpty()
                .Length(1, 512);
        }
    }
}
