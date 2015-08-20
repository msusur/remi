using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.BusinessRules;

namespace ReMi.QueryValidators.BusinessRules
{
    public class GetBusinessRuleValidator : RequestValidatorBase<GetBusinessRuleRequest>
    {
        public GetBusinessRuleValidator()
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.Name) || x.ExternalId.HasValue)
                .WithName("Name or ExternalId")
                .WithMessage("Rule Name or ExternalId has to be provided");
        }
    }
}
