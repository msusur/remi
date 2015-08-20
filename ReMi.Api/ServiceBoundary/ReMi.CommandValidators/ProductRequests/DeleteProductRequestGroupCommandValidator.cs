using System;
using FluentValidation;
using ReMi.Commands.ProductRequests;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ProductRequests
{
    public class DeleteProductRequestGroupCommandValidator : RequestValidatorBase<DeleteProductRequestGroupCommand>
    {
        public DeleteProductRequestGroupCommandValidator()
        {
            RuleFor(o => o.RequestGroupId).NotEqual(Guid.Empty);
        }
    }
}
