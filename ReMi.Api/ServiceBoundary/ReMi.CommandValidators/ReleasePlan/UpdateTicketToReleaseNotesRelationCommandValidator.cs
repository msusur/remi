using System;
using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.CommandValidators.Common;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class UpdateTicketToReleaseNotesRelationCommandValidator : RequestValidatorBase<UpdateTicketToReleaseNotesRelationCommand>
    {
        public UpdateTicketToReleaseNotesRelationCommandValidator()
        {
            RuleFor(command => command.Tickets).SetCollectionValidator(new TicketValidator());
            RuleFor(command => command.ReleaseWindowId).NotEmpty().Must(x => x != Guid.Empty);
        }
    }
}
