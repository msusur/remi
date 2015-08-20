using System;
using System.Linq;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.
DataAccess.BusinessEntityGateways.BusinessRules;
using ReMi.Queries.BusinessRules;

namespace ReMi.QueryHandlers.BusinessRules
{
    public class GetBusinessRulesHandler : IHandleQuery<GetBusinessRulesRequest, GetBusinessRulesResponse>
    {
        public Func<IBusinessRuleGateway> BusinessRuleGatewayFactory { get; set; }

        public GetBusinessRulesResponse Handle(GetBusinessRulesRequest request)
        {
            using (var gateway = BusinessRuleGatewayFactory())
            {
                return new GetBusinessRulesResponse
                {
                    Rules = gateway.GetBusinessRules()
                        .GroupBy(x => x.Group)
                        .ToDictionary(x => x.Key, x => x.Select(y => y))
                };
            }
        }
    }
}
