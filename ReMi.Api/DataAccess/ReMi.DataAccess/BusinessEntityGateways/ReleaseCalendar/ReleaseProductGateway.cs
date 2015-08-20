using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar
{
    public class ReleaseProductGateway : IReleaseProductGateway
    {
        public IRepository<ReleaseProduct> ReleaseProductRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<Product> ProductRepository { get; set; }

        public void AssignProductsToRelease(Guid releaseWindowId, IEnumerable<string> products)
        {
            if(products == null)
                throw new ArgumentNullException("products");

            var assignedProducts = products as IList<string> ?? products.ToList();

            var existingRefs = ReleaseProductRepository
                    .GetAllSatisfiedBy(x => x.ReleaseWindow.ExternalId == releaseWindowId)
                    .Select(x => x.Product.Description)
                    .ToArray();

            var newProductNames = assignedProducts.Where(x => existingRefs.All(o => o != x)).ToList();
            if (newProductNames.Any())
            {
                var newProducts =
                    ProductRepository.GetAllSatisfiedBy(x => newProductNames.Contains(x.Description)).ToList();

                if (newProducts.Any())
                {
                    var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);
                    if (releaseWindow == null)
                        throw new ReleaseWindowNotFoundException(releaseWindowId);

                    foreach (var newProduct in newProducts)
                    {
                        ReleaseProductRepository.Insert(new ReleaseProduct
                        {
                            CreatedOn = SystemTime.Now,
                            ProductId = newProduct.ProductId,
                            ReleaseWindowId = releaseWindow.ReleaseWindowId
                        });
                    }
                }
            }
        }

        public void Dispose()
        {
            ReleaseProductRepository.Dispose();
            ReleaseWindowRepository.Dispose();
            ProductRepository.Dispose();
        }
    }
}
