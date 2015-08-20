using System;
using System.Collections.Generic;
using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.BusinessRules;

namespace ReMi.QueryValidators.BusinessRules
{
    public class TriggerBusinessRuleValidator : RequestValidatorBase<TriggerBusinessRuleRequest>
    {
        public TriggerBusinessRuleValidator()
        {
            RuleFor(x => x)
                .Must(x => x.ExternalId != Guid.Empty || !string.IsNullOrEmpty(x.Rule))
                .WithName("Rule or ExternalId")
                .WithMessage("Rule name and Group or ExternalId are required");
            RuleFor(x => x.Parameters).SetCollectionValidator(new ParameterValidator());
        }

        private class ParameterValidator : RequestValidatorBase<KeyValuePair<string, string>>
        {
            public ParameterValidator()
            {
                RuleFor(x => x.Key).NotEmpty();
                RuleFor(x => x.Value).NotEmpty();
            }
        }
    }
}
