using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class UpdateTicketCommentCommandValidator : RequestValidatorBase<UpdateTicketCommentCommand>
    {
        public UpdateTicketCommentCommandValidator()
        {
            RuleFor(x => x.TicketId).NotEmpty();
            RuleFor(x => x.TicketKey).NotEmpty();
        }
    }
}
