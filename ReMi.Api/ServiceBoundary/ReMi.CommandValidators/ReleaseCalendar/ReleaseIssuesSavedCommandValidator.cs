using System;
using FluentValidation;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleaseCalendar
{
    public class ReleaseIssuesSavedCommandValidator : RequestValidatorBase<SaveReleaseIssuesCommand>
    {
        public ReleaseIssuesSavedCommandValidator()
        {
            RuleFor(x => x.ReleaseWindow)
                .NotNull()
                .Must(x => x.ExternalId != Guid.Empty);

            RuleFor(x => x.ReleaseWindow.Issues).Length(0, 8172);
        }
    }
}
