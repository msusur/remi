using System;
using FluentValidation;
using ReMi.Commands.ProductRequests;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ProductRequests
{
    public class CreateProductRequestTypeCommandValidator : RequestValidatorBase<CreateProductRequestTypeCommand>
    {
        public CreateProductRequestTypeCommandValidator()
        {
            RuleFor(o => o.RequestType).NotNull();

            RuleFor(o => o.RequestType.Name).NotEmpty();

            RuleFor(o => o.RequestType.Name).Length(1, 1024).WithMessage("Name should be less then 1024");

            RuleFor(o => o.RequestType.ExternalId).NotEqual(Guid.Empty);
        }
    }
}
