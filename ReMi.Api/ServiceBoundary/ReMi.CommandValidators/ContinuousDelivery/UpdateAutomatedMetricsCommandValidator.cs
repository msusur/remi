using System;
using FluentValidation;
using ReMi.Commands.ContinuousDelivery;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ContinuousDelivery
{
    public class UpdateAutomatedMetricsCommandValidator : RequestValidatorBase<UpdateAutomatedMetricsCommand>
    {
        public UpdateAutomatedMetricsCommandValidator()
        {
            RuleFor(r => r.ReleaseWindowId).Must(x => x != Guid.Empty);
        }
    }
}
