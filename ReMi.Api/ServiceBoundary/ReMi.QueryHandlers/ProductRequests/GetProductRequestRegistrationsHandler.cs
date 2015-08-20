using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using ReMi.Queries.ProductRequests;

namespace ReMi.QueryHandlers.ProductRequests
{
    public class GetProductRequestRegistrationsHandler : IHandleQuery<GetProductRequestRegistrationsRequest, GetProductRequestRegistrationsResponse>
    {
        public Func<IProductRequestRegistrationGateway> ProductRequestRegistrationGatewayFactory { get; set; }

        public GetProductRequestRegistrationsResponse Handle(GetProductRequestRegistrationsRequest request)
        {
            using (var gateway = ProductRequestRegistrationGatewayFactory())
                return new GetProductRequestRegistrationsResponse
                {
                    Registrations = gateway.GetRegistrations()
                };
        }
    }
}
