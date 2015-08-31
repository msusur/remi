using FluentValidation;
using ReMi.Commands.Configuration;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Configuration
{
    public class RemoveBusinessUnitCommandValidator : RequestValidatorBase<RemoveBusinessUnitCommand>
    {
        public RemoveBusinessUnitCommandValidator()
        {
            RuleFor(c => c.ExternalId).NotEmpty();
        }
    }
}
