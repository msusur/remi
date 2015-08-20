using System;
using FluentValidation;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleaseCalendar
{
    public class UpdateReleaseDecisionCommandHandlerValidator : RequestValidatorBase<UpdateReleaseDecisionCommand>
    {
        public UpdateReleaseDecisionCommandHandlerValidator()
        {
            RuleFor(o => o.ReleaseWindowId).Must(x => x != Guid.Empty);
        }
    }
}
