using System;
using FluentValidation;
using ReMi.Commands.ProductRequests;
using ReMi.CommandValidators.Common;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ProductRequests
{
    public class UpdateProductRequestGroupCommandValidator : RequestValidatorBase<UpdateProductRequestGroupCommand>
    {
        public UpdateProductRequestGroupCommandValidator()
        {
            RuleFor(o => o.RequestGroup).NotNull();

            RuleFor(o => o.RequestGroup.Name).NotEmpty();

            RuleFor(o => o.RequestGroup.Name).Length(1, 1024).WithMessage("Name should be less then 1024");

            RuleFor(o => o.RequestGroup.ExternalId).NotEqual(Guid.Empty);

            RuleFor(o => o.RequestGroup)
                .SetValidator(new ProductRequestAssigneesValidator());
        }
    }
}
