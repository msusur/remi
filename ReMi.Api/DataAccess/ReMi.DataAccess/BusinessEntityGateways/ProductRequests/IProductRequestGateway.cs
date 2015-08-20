using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ProductRequests;

namespace ReMi.DataAccess.BusinessEntityGateways.ProductRequests
{
    public interface IProductRequestGateway : IDisposable
    {
        IEnumerable<ProductRequestType> GetRequestTypes();
        ProductRequestType GetRequestType(Guid externalId);

        void CreateProductRequestType(ProductRequestType type);
        void UpdateProductRequestType(ProductRequestType type);
        void DeleteProductRequestType(Guid productRequestTypeId);

        void CreateProductRequestGroup(ProductRequestGroup group);
        void UpdateProductRequestGroup(ProductRequestGroup group);
        void DeleteProductRequestGroup(Guid productRequestGroupId);

        void CreateProductRequestTask(ProductRequestTask task);
        void UpdateProductRequestTask(ProductRequestTask task);
        void DeleteProductRequestTask(Guid productRequestTaskId);

        IEnumerable<ProductRequestGroup> GetRequestGroupsByTasks(IEnumerable<Guid> taskIds);
    }
}
