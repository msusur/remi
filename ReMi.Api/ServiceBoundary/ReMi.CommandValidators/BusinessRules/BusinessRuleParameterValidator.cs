using FluentValidation;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.Common.Cqrs.FluentValidation;
using System;

namespace ReMi.CommandValidators.BusinessRules
{
    internal class BusinessRuleParameterValidator : RequestValidatorBase<BusinessRuleParameter>
    {
        internal BusinessRuleParameterValidator()
        {
            RuleFor(x => x.ExternalId).NotEqual(Guid.Empty);
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.TestData).NotNull();
            RuleFor(x => x.TestData.ExternalId).NotEqual(Guid.Empty);
            RuleFor(x => x.TestData.JsonData).NotEmpty();
        }
    }
}
