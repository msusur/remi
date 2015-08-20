using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.BusinessRules;

namespace ReMi.QueryValidators.BusinessRules
{
    public class GetGeneratedRuleValidator : RequestValidatorBase<GetGeneratedRuleRequest>
    {
        public GetGeneratedRuleValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Namespace).NotEmpty();
        }
    }
}
