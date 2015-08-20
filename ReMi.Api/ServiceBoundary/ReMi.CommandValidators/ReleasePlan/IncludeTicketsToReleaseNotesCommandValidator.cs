using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class IncludeTicketsToReleaseNotesCommandValidator : RequestValidatorBase<IncludeTicketsToReleaseNotesCommand>
    {
        public IncludeTicketsToReleaseNotesCommandValidator()
        {
            RuleFor(c => c.TicketIds).NotNull().Must(x => x.Count > 0);
        }
    }
}
