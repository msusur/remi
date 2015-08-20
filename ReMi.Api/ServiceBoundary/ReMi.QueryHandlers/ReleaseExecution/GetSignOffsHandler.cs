using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.Queries.ReleaseExecution;

namespace ReMi.QueryHandlers.ReleaseExecution
{
    public class GetSignOffsHandler : IHandleQuery<GetSignOffsRequest, GetSignOffsResponse>
    {
        public Func<ISignOffGateway> SignOffGatewayFactory { get; set; }

        public GetSignOffsResponse Handle(GetSignOffsRequest request)
        {
            List<SignOff> signOffs;

            using (var gateway = SignOffGatewayFactory())
            {
                signOffs = gateway.GetSignOffs(request.ReleaseWindowId);
            }

            return new GetSignOffsResponse
            {
                SignOffs = signOffs
            };
        }
    }
}
