using System;
using FluentValidation;
using ReMi.Commands.ProductRequests;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ProductRequests
{
    public class UpdateProductRequestTaskCommandValidator : RequestValidatorBase<UpdateProductRequestTaskCommand>
    {
        public UpdateProductRequestTaskCommandValidator()
        {
            RuleFor(o => o.RequestTask).NotNull();

            RuleFor(o => o.RequestTask.Question).NotEmpty();

            RuleFor(o => o.RequestTask.ExternalId).NotEqual(Guid.Empty);
        }
    }
}
