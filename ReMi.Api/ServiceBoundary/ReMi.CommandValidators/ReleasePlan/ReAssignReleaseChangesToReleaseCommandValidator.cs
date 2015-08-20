using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class ReAssignReleaseChangesToReleaseCommandValidator : RequestValidatorBase<ReAssignReleaseChangesToReleaseCommand>
    {
        public ReAssignReleaseChangesToReleaseCommandValidator()
        {
            RuleFor(x => x.ReleaseWindowId).NotEmpty();
        }
    }
}
