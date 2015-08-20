using System;
using FluentValidation;
using ReMi.Commands.Metrics;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Metrics
{
    public class UpdateMetricsCommandValidator : RequestValidatorBase<UpdateMetricsCommand>
    {
        public UpdateMetricsCommandValidator()
        {
            RuleFor(r => r.ReleaseWindowId).Must(x => x != Guid.Empty);

            RuleFor(r => r.Metric.ExternalId).Must(x => x != Guid.Empty);
        }
    }
}
