using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleaseParticipant;

namespace ReMi.QueryHandlers.ReleaseParticipant
{
    public class GetReleaseParticipantsHandler : IHandleQuery<GetReleaseParticipantRequest, GetReleaseParticipantResponse>
    {
        public Func<IReleaseParticipantGateway> ReleaseParticipantGatewayFactory { get; set; }
 
        public GetReleaseParticipantResponse Handle(GetReleaseParticipantRequest request)
        {
            using (var gateway = ReleaseParticipantGatewayFactory())
            {
                return new GetReleaseParticipantResponse
                {
                    Participants = gateway.GetReleaseParticipants(request.ReleaseWindowId)
                };
            }
        }
    }
}
