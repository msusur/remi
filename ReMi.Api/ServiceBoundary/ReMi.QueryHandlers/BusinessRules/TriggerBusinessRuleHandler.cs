using System;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.BusinessRules;

namespace ReMi.QueryHandlers.BusinessRules
{
    public class TriggerBusinessRuleHandler : IHandleQuery<TriggerBusinessRuleRequest, TriggerBusinessRuleResponse>
    {
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        public TriggerBusinessRuleResponse Handle(TriggerBusinessRuleRequest request)
        {
            var response = new TriggerBusinessRuleResponse ();

            if (request.ExternalId != Guid.Empty)
            {
                response.Result = BusinessRuleEngine.Execute(
                    request.Context.UserId,
                    request.ExternalId,
                    request.Parameters);
            }
            else
            {
                response.Result = BusinessRuleEngine.Execute(
                    request.Context.UserId,
                    request.Group,
                    request.Rule,
                    request.Parameters);
            }
            return response;
        }
    }
}
