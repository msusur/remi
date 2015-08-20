using System;
using FluentValidation;
using ReMi.Commands.ProductRequests;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ProductRequests
{
    public class DeleteProductRequestRegistrationCommandValidator : RequestValidatorBase<DeleteProductRequestRegistrationCommand>
    {
        public DeleteProductRequestRegistrationCommandValidator()
        {
            RuleFor(o => o.RegistrationId).NotEqual(Guid.Empty);
        }
    }
}
