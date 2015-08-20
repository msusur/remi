using FluentValidation;
using ReMi.Commands.Configuration;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Configuration
{
    public class UpdateProductCommandValidator : RequestValidatorBase<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(c => c.ExternalId).NotEmpty();
            RuleFor(c => c.Description).NotEmpty();
            RuleFor(c => c.BusinessUnitId).NotEmpty();
        }
    }
}
