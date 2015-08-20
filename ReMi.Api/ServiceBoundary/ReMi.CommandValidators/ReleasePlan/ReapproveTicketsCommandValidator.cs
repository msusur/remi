using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class ReapproveTicketsCommandValidator : RequestValidatorBase<ReapproveTicketsCommand>
    {
        public ReapproveTicketsCommandValidator()
        {
            RuleFor(x => x.ReleaseWindowId).NotEmpty();
        }
    }
}
