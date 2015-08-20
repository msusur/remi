using System;
using FluentValidation;
using ReMi.Commands.ProductRequests;
using ReMi.CommandValidators.Common;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ProductRequests
{
    public class UpdateProductRequestRegistrationCommandValidator : RequestValidatorBase<UpdateProductRequestRegistrationCommand>
    {
        public UpdateProductRequestRegistrationCommandValidator()
        {
            RuleFor(o => o.Registration).NotNull();

            RuleFor(o => o.Registration.Description).NotEmpty();

            RuleFor(o => o.Registration.Description).Length(1, 1024);

            RuleFor(o => o.Registration.ExternalId).NotEqual(Guid.Empty);

            RuleFor(o => o.Registration.ProductRequestTypeId).NotEmpty();

            RuleFor(o => o.Registration)
                .SetValidator(new ProductRequestRegistrationTaskValidator());
        }
    }
}
