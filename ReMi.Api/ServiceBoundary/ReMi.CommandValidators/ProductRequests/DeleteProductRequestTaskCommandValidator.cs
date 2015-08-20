using System;
using FluentValidation;
using ReMi.Commands.ProductRequests;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ProductRequests
{
    public class DeleteProductRequestTaskCommandValidator : RequestValidatorBase<DeleteProductRequestTaskCommand>
    {
        public DeleteProductRequestTaskCommandValidator()
        {
            RuleFor(o => o.RequestTaskId).NotEqual(Guid.Empty);
        }
    }
}
