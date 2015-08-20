using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Products;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Queries.Configuration;

namespace ReMi.QueryHandlers.Configuration
{
    public class GetProductsHandler : IHandleQuery<GetProductsRequest, GetProductsResponse>
    {
        public Func<IProductGateway> ProductGatewayFactory { get; set; }

        public GetProductsResponse Handle(GetProductsRequest request)
        {
            List<ProductView> result;
            using (var gateway = ProductGatewayFactory())
            {
                result = gateway.GetAllProducts().ToList();
            }

            return new GetProductsResponse
            {
                Products = result
            };
        }
    }
}
