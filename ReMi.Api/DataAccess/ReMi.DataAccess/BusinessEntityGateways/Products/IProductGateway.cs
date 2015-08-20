using ReMi.BusinessEntities.Products;
using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.BusinessEntityGateways.Products
{
    public interface IProductGateway : IDisposable
    {
        IEnumerable<Product> GetProducts(Guid releaseWindowId);
        IEnumerable<Product> GetProducts(IEnumerable<string> productNames);

        Product GetProduct(string name);
        IEnumerable<ProductView> GetAllProducts();

        void AddProduct(Product product);
        void UpdateProduct(Product product);
    }
}
