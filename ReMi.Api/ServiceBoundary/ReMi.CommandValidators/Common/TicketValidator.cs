using FluentValidation;
using ReMi.BusinessEntities.ReleasePlan;

namespace ReMi.CommandValidators.Common
{
    public class TicketValidator : AbstractValidator<ReleaseContentTicket>
    {
        public TicketValidator()
        {
            RuleFor(q => q.TicketId).NotEmpty();
            RuleFor(q => q.TicketName).NotEmpty();
        }
    }
}
