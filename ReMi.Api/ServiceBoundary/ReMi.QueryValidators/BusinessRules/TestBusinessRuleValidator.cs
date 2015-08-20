using System;
using FluentValidation;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.BusinessRules;

namespace ReMi.QueryValidators.BusinessRules
{
    public class TestBusinessRuleValidator : RequestValidatorBase<TestBusinessRuleRequest>
    {
        public TestBusinessRuleValidator()
        {
            RuleFor(x => x.Rule).NotNull();
            RuleFor(x => x.Rule.Description).NotEmpty();
            RuleFor(x => x.Rule.ExternalId).NotEqual(Guid.Empty);
            RuleFor(x => x.Rule.Name).NotEmpty();
            RuleFor(x => x.Rule.Script).NotEmpty();
            RuleFor(x => x.Rule.AccountTestData).NotNull();
            RuleFor(x => x.Rule.AccountTestData.ExternalId).NotEqual(Guid.Empty);
            RuleFor(x => x.Rule.AccountTestData.JsonData).NotEmpty();
            RuleFor(x => x.Rule.Parameters).SetCollectionValidator(new BusinessRuleParameterValidator());
        }

        private class BusinessRuleParameterValidator : RequestValidatorBase<BusinessRuleParameter>
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
}
