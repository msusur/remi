using FluentValidation;
using ReMi.Commands.Configuration;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Configuration
{
    public class AddBusinessUnitCommandValidator : RequestValidatorBase<AddBusinessUnitCommand>
    {
        public AddBusinessUnitCommandValidator()
        {
            RuleFor(c => c.ExternalId).NotEmpty();
            RuleFor(c => c.Description).NotEmpty();
            RuleFor(c => c.Name).NotEmpty();
        }
    }
}
