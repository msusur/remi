using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.BusinessRules;
using ReMi.Queries.BusinessRules;

namespace ReMi.QueryHandlers.BusinessRules
{
    public class GetBusinessRuleHandler : IHandleQuery<GetBusinessRuleRequest, GetBusinessRuleResponse>
    {
        public Func<IBusinessRuleGateway> BusinessRuleGatewayFactory { get; set; }

        public GetBusinessRuleResponse Handle(GetBusinessRuleRequest request)
        {
            using (var gateway = BusinessRuleGatewayFactory())
            {
                return new GetBusinessRuleResponse
                {
                    Rule = request.ExternalId.HasValue
                        ? gateway.GetBusinessRule(request.ExternalId.Value) : gateway.GetBusinessRule(request.Group, request.Name)
                };
            }
        }
    }
}
