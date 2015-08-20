using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetTicketRiskHandler : IHandleQuery<GetTicketRiskRequest, GetTicketRiskResponse>
    {
        public Func<IReleaseContentGateway> ReleaseContentGateway { get; set; }

        public GetTicketRiskResponse Handle(GetTicketRiskRequest request)
        {
            using (var gateway = ReleaseContentGateway())
            {
                return new GetTicketRiskResponse
                {
                    TicketRisk = gateway.GetTicketRisk()
                };
            }
        }
    }
}
