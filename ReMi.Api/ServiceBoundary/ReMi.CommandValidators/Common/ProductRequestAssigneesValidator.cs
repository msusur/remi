using System;
using FluentValidation;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Common
{
    public class ProductRequestAssigneesValidator : AbstractValidator<ProductRequestGroup>
    {
        public ProductRequestAssigneesValidator()
        {
            RuleFor(group => group.Assignees)
                .SetCollectionValidator(new AccountValidator())
                .WithMessage("Assignees are invalid");
        }
    }

    public class AccountValidator : RequestValidatorBase<Account>
    {
        public AccountValidator()
        {
            RuleFor(x => x.ExternalId).Must(x => x != Guid.Empty);
        }
    }
}
