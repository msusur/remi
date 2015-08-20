using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.Products;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Queries.Auth;

namespace ReMi.QueryHandlers.Auth
{
    public class GetAccountHandler : IHandleQuery<GetAccountRequest, GetAccountResponse>
    {
        public Func<IProductGateway> ProductGatewayFactory { get; set; }
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }

        public GetAccountResponse Handle(GetAccountRequest request)
        {
            Account result;
            using (var gateway = AccountsGatewayFactory())
            {
                result = gateway.GetAccount(request.AccountId);
            }

            IEnumerable<ProductView> products = null;
            if (result != null)
            {
                if (!result.Products.IsNullOrEmpty())
                {
                    if (!result.Products.Any(x => x.IsDefault))
                        result.Products.First().IsDefault = true;
                    else if (result.Products.Count(x => x.IsDefault) > 1)
                        throw new MoreThanOneDefaultProductException(result);
                }
                if (result.Role.Name == "Admin")
                {
                    using (var productsGateway = ProductGatewayFactory())
                    {
                        products = productsGateway.GetAllProducts();
                        if (!products.IsNullOrEmpty())
                        {
                            ProductView defaultProduct = null;
                            if (!result.Products.IsNullOrEmpty())
                            {
                                var productId = result.Products.First(x => x.IsDefault).ExternalId;
                                defaultProduct = products.FirstOrDefault(x => x.ExternalId == productId);
                            }
                            else if (defaultProduct == null)
                            {
                                defaultProduct = products.First();
                            }
                            defaultProduct.IsDefault = true;
                            result.Products = products;
                        }
                    }
                }
            }
            return new GetAccountResponse
            {
                Account = result
            };
        }
    }
}
