using System;
using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class RemoveReleaseApproverCommandValidator : RequestValidatorBase<RemoveReleaseApproverCommand>
    {
        public RemoveReleaseApproverCommandValidator()
        {
            RuleFor(o => o.ApproverId).Must(x => x != Guid.Empty).WithMessage("ApproverId should not be empty");
            RuleFor(o => o.ReleaseWindowId).Must(x => x != Guid.Empty).WithMessage("ReleaseWindowId should not be empty");
        }
    }
}
