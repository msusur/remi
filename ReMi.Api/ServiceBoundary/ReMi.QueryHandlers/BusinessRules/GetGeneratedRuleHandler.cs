using ReMi.BusinessEntities.BusinessRules;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.BusinessRules;

namespace ReMi.QueryHandlers.BusinessRules
{
    public class GetGeneratedRuleHandler : IHandleQuery<GetGeneratedRuleRequest, GetGeneratedRuleResponse>
    {
        public IBusinessRuleGenerator BusinessRuleGenerator { get; set; }
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        public GetGeneratedRuleResponse Handle(GetGeneratedRuleRequest request)
        {
            var type = BusinessRuleEngine.GetType(string.Format("{0}.{1}", request.Namespace, request.Name));

            if (type == null)
                return new GetGeneratedRuleResponse { Rule = null };

            BusinessRuleDescription rule = null;

            if (typeof(ICommand).IsAssignableFrom(type))
            {
                rule = BusinessRuleGenerator.GenerateCommandRule(type, request.Context.UserId);
            }
            else if (typeof(IQuery).IsAssignableFrom(type))
            {
                rule = BusinessRuleGenerator.GenerateQueryRule(type, request.Context.UserId);
            }

            return new GetGeneratedRuleResponse
            {
                Rule = rule
            };
        }
    }
}
