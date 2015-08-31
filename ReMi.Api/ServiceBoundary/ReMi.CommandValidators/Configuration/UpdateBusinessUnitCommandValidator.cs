using FluentValidation;
using ReMi.Commands.Configuration;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Configuration
{
    public class UpdateBusinessUnitCommandValidator : RequestValidatorBase<UpdateBusinessUnitCommand>
    {
        public UpdateBusinessUnitCommandValidator()
        {
            RuleFor(c => c.ExternalId).NotEmpty();
            RuleFor(c => c.Description).NotEmpty();
            RuleFor(c => c.Name).NotEmpty();
        }
    }
}
