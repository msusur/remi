using System;
using System.Linq;
using FluentValidation;
using ReMi.Commands.Auth;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Auth
{
    public class CheckAccountsCommandValidator : RequestValidatorBase<CheckAccountsCommand>
    {
        public CheckAccountsCommandValidator()
        {
            RuleFor(o => o.ReleaseWindowId).NotEmpty();

            RuleFor(o => o.Accounts).NotNull();

            RuleFor(o => o.Accounts).Must(x => x.Any());

            RuleForEach(o => o.Accounts).Must(o => o.ExternalId != Guid.Empty
                && !string.IsNullOrWhiteSpace(o.Email) && !string.IsNullOrWhiteSpace(o.Name) && !string.IsNullOrWhiteSpace(o.FullName));
        }
    }
}
