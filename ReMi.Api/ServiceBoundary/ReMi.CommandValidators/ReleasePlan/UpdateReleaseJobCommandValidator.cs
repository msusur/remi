using System;
using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class UpdateReleaseJobCommandValidator : RequestValidatorBase<UpdateReleaseJobCommand>
    {
        public UpdateReleaseJobCommandValidator()
        {
            RuleFor(o => o.ReleaseWindowId).NotEmpty();

            RuleFor(o => o.ReleaseJob).NotNull();
            When(x => x.ReleaseJob != null, () => {
                RuleFor(o => o.ReleaseJob.JobId).NotEmpty();
                RuleFor(o => o.ReleaseJob.Name).NotEmpty();
                RuleFor(o => o.ReleaseJob.ExternalId).NotEmpty();
            });
        }
    }
}
