using System;
using System.Collections.Generic;
using FluentValidation;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.ReleasePlan
{
    public class UpdateReleaseTasksOrderCommandValidator : RequestValidatorBase<UpdateReleaseTasksOrderCommand>
    {
        public UpdateReleaseTasksOrderCommandValidator()
        {
            RuleFor(x => x.ReleaseTasksOrder)
                .NotNull()
                .NotEmpty()
                .SetCollectionValidator(new OrderNumberValidator());
        }

        private class OrderNumberValidator : RequestValidatorBase<KeyValuePair<Guid, short>>
        {
            public OrderNumberValidator()
            {
                RuleFor(x => x.Key).Must(x => x != Guid.Empty);
                RuleFor(x => x.Value).GreaterThanOrEqualTo((short)0);
            }
        }

    }
}
