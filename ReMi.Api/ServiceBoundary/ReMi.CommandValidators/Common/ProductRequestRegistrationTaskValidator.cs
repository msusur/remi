using System;
using FluentValidation;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Common
{
    public class ProductRequestRegistrationTaskValidator : AbstractValidator<ProductRequestRegistration>
    {
        public ProductRequestRegistrationTaskValidator()
        {
            RuleFor(group => group.Tasks)
                .SetCollectionValidator(new ProductRequestTaskValidator())
                .WithMessage("Tasks are invalid");
        }
    }

    public class ProductRequestTaskValidator : RequestValidatorBase<ProductRequestRegistrationTask>
    {
        public ProductRequestTaskValidator()
        {
            RuleFor(x => x.ProductRequestTaskId).Must(x => x != Guid.Empty);
        }
    }
}
