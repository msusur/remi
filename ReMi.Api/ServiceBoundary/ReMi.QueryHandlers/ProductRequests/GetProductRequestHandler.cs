using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using ReMi.Queries.ProductRequests;

namespace ReMi.QueryHandlers.ProductRequests
{
    public class GetProductRequestHandler : IHandleQuery<GetProductRequestsRequest, GetProductRequestsResponse>
    {
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }

        public GetProductRequestsResponse Handle(GetProductRequestsRequest request)
        {
            using (var gateway = ProductRequestGatewayFactory())
            {
                return new GetProductRequestsResponse
                {
                    ProductRequestTypes = gateway.GetRequestTypes()
                };
            }
        }
    }
}
