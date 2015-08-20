using System;
using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class AddReleaseApproversCommandValidator : RequestValidatorBase<AddReleaseApproversCommand>
    {
        public AddReleaseApproversCommandValidator()
        {
            RuleFor(o => o.Approvers).NotNull();

            RuleFor(o => o.Approvers).Must(o => o.Length > 0);

            RuleForEach(o => o.Approvers)
                .NotEmpty()
                .Must(x => x.ExternalId != Guid.Empty && x.ReleaseWindowId != Guid.Empty);
        }
    }
}
