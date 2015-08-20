using System;
using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class UpdateTicketRiskCommandValidator : RequestValidatorBase<UpdateTicketRiskCommand>
    {
        public UpdateTicketRiskCommandValidator()
        {
            RuleFor(x => x.TicketId).NotEmpty();
            RuleFor(x => x.TicketKey).NotEmpty();
            RuleFor(x => x.Risk).NotEmpty()
                .Must(x =>
                {
                    TicketRisk ticketRisk;
                    return Enum.TryParse(x, out ticketRisk);
                });
        }
    }
}
