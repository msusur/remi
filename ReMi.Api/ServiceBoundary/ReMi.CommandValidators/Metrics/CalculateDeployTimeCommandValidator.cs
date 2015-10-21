using FluentValidation;
using ReMi.Commands.Metrics;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Metrics
{
    public class CalculateDeployTimeCommandValidator : RequestValidatorBase<CalculateDeployTimeCommand>
    {
        public CalculateDeployTimeCommandValidator()
        {
            RuleFor(x => x.ReleaseWindowId).NotEmpty();
        }
    }
}
