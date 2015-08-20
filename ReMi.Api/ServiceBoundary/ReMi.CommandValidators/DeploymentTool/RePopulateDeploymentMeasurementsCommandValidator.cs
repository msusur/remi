using FluentValidation;
using ReMi.Commands.DeploymentTool;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.DeploymentTool
{
    public class RePopulateDeploymentMeasurementsCommandValidator : RequestValidatorBase<RePopulateDeploymentMeasurementsCommand>
    {
        public RePopulateDeploymentMeasurementsCommandValidator()
        {
            RuleFor(request => request.ReleaseWindowId).NotEmpty();
        }
    }
}
