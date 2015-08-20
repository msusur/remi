using System;
using FluentValidation;
using ReMi.Commands.Metrics;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Metrics
{
    public class UpdateReleaseMetricsCommandValidator : RequestValidatorBase<UpdateReleaseMetricsCommand>
    {
        public UpdateReleaseMetricsCommandValidator()
        {
            RuleFor(x => x.ReleaseWindow.ExternalId).Must(x => x != Guid.Empty);
        }
    }
}
