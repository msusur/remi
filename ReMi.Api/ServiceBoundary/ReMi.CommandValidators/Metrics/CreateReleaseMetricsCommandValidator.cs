using System;
using FluentValidation;
using ReMi.Commands.Metrics;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Metrics
{
    public class CreateReleaseMetricsCommandValidator : RequestValidatorBase<CreateReleaseMetricsCommand>
    {
        public CreateReleaseMetricsCommandValidator()
        {
            RuleFor(x => x.ReleaseWindow).Must(x => x.ExternalId != Guid.Empty);
        }
    }
}
