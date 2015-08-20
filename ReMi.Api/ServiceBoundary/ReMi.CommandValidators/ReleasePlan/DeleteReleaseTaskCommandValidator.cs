using System;
using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class DeleteReleaseTaskCommandValidator : RequestValidatorBase<DeleteReleaseTaskCommand>
    {
        public DeleteReleaseTaskCommandValidator()
        {
            RuleFor(x => x.ReleaseTaskId).Must(x => x != Guid.Empty);
        }
    }
}
