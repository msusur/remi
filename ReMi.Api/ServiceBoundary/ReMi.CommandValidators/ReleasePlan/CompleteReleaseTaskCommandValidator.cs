using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class CompleteReleaseTaskCommandValidator : RequestValidatorBase<CompleteReleaseTaskCommand>
    {
        public CompleteReleaseTaskCommandValidator()
        {
            RuleFor(request => request.ReleaseTaskExtetnalId).NotEmpty();
            RuleFor(request => request.CommandContext).NotNull().NotEmpty();
            RuleFor(request => request.CommandContext.UserId).NotEmpty();
        }
    }
}
