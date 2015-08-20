using FluentValidation;
using ReMi.Commands.ContinuousDelivery;
using ReMi.Common.Cqrs.FluentValidation;
using System;

namespace ReMi.CommandValidators.ContinuousDelivery
{
    public class CreateAutomatedReleaseWindowCommandValidator : RequestValidatorBase<CreateAutomatedReleaseWindowCommand>
    {
        public CreateAutomatedReleaseWindowCommandValidator()
        {
            RuleFor(request => request.Product).NotNull().NotEmpty();

            RuleFor(x => x.Description)
                .Length(0, 1024);

            RuleFor(request => request.ExternalId)
                .Must(x => x != Guid.Empty)
                .WithMessage("ExternalId cannot be empty");
        }
    }
}
