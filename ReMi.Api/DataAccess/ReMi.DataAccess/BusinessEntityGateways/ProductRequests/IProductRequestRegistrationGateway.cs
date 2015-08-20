using ReMi.Common.Constants.ProductRequests;
using System;
using System.Collections.Generic;
using ProductRequestRegistration = ReMi.BusinessEntities.ProductRequests.ProductRequestRegistration;

namespace ReMi.DataAccess.BusinessEntityGateways.ProductRequests
{
    public interface IProductRequestRegistrationGateway : IDisposable
    {
        ProductRequestRegistration GetRegistration(Guid externalId);
        IEnumerable<ProductRequestRegistration> GetRegistrations();

        void CreateProductRequestRegistration(ProductRequestRegistration registration);
        void UpdateProductRequestRegistration(ProductRequestRegistration registration);
        void DeleteProductRequestRegistration(Guid productRequestRegistrationId, RemovingReason removingReason, string comment);
    }
}
