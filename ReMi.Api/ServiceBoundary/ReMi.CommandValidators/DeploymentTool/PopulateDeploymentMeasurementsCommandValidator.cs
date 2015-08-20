using FluentValidation;
using ReMi.Commands.DeploymentTool;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.DeploymentTool
{
    public class PopulateDeploymentMeasurementsCommandValidator : RequestValidatorBase<PopulateDeploymentMeasurementsCommand>
    {
        public PopulateDeploymentMeasurementsCommandValidator()
        {
            RuleFor(request => request.ReleaseWindowId).NotEmpty();
        }
    }
}
