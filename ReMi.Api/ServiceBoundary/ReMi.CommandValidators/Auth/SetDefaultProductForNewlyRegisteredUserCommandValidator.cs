using System;
using System.Linq;
using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Auth
{
    public class SetDefaultProductForNewlyRegisteredUserCommandValidator : 
        RequestValidatorBase<SetDefaultProductForNewlyRegisteredUserCommand>
    {
        public SetDefaultProductForNewlyRegisteredUserCommandValidator()
        {
            RuleFor(x => x.Account).NotNull();

            RuleFor(o => o.Account)
                .Must(x => x.ExternalId != Guid.Empty)
                .WithMessage("Account external id should not be empty");

            RuleFor(o => o.Account.Products)
                .Must(x => x.Count() == 1 && x.ToList()[0].IsDefault)
                .WithMessage("Default product should be present");
        }
    }
}
