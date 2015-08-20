using ReMi.BusinessEntities.Products;
using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.BusinessEntityGateways.Products
{
    public interface IBusinessUnitsGateway : IDisposable
    {
        IEnumerable<Product> GetPackages(Guid accountId, bool includeAll = false);
        Product GetDefaultPackage(Guid accountId);
        IEnumerable<BusinessUnit> GetEmptyBusinessUnits();
    }
}
