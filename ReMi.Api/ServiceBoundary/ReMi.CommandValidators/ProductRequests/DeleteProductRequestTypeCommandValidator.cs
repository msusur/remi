using System;
using FluentValidation;
using ReMi.Commands.ProductRequests;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ProductRequests
{
    public class DeleteProductRequestTypeCommandValidator : RequestValidatorBase<DeleteProductRequestTypeCommand>
    {
        public DeleteProductRequestTypeCommandValidator()
        {
            RuleFor(o => o.RequestTypeId).NotEqual(Guid.Empty);
        }
    }
}
