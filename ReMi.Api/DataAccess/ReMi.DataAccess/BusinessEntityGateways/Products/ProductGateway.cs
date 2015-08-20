using AutoMapper;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Exceptions.Configuration;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;

namespace ReMi.DataAccess.BusinessEntityGateways.Products
{
    public class ProductGateway : BaseGateway, IProductGateway
    {
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<Product> ProductRepository { get; set; }
        public IRepository<BusinessUnit> BusinessUnitRepository { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public IEnumerable<BusinessEntities.Products.Product> GetProducts(Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);

            if (releaseWindow == null)
                throw new ReleaseWindowNotFoundException(releaseWindowId);

            var products = releaseWindow.ReleaseProducts.Select(o => o.Product).ToList();

            return MappingEngine.Map<IEnumerable<Product>, IEnumerable<BusinessEntities.Products.Product>>(products);
        }

        public IEnumerable<BusinessEntities.Products.Product> GetProducts(IEnumerable<string> productNames)
        {
            if (productNames == null)
                throw new ArgumentNullException("productNames");

            var localProducts = productNames as string[] ?? productNames.ToArray();

            var products = ProductRepository.GetAllSatisfiedBy(x => localProducts.Contains(x.Description)).ToList();

            return MappingEngine.Map<IEnumerable<Product>, IEnumerable<BusinessEntities.Products.Product>>(products);
        }

        public BusinessEntities.Products.Product GetProduct(string name)
        {
            var product = ProductRepository.GetSatisfiedBy(x => string.Equals(x.Description, name));
            if (product == null)
                throw new ProductNotFoundException(name);

            return MappingEngine.Map<Product, BusinessEntities.Products.Product>(product);
        }

        public IEnumerable<BusinessEntities.Products.ProductView> GetAllProducts()
        {
            var products = ProductRepository.Entities;

            return MappingEngine.Map<IEnumerable<Product>, IEnumerable<BusinessEntities.Products.ProductView>>(products);
        }

        public void AddProduct(BusinessEntities.Products.Product product)
        {
            var businessUnit =
                BusinessUnitRepository.GetSatisfiedBy(x => x.ExternalId == product.BusinessUnit.ExternalId);
            if (businessUnit == null)
            {
                throw new EntityNotFoundException(typeof(BusinessUnit), product.BusinessUnit.ExternalId);
            }
            if (ProductRepository.Entities.Any(p => p.ExternalId == product.ExternalId))
            {
                throw new ProductAlreadyExistsException(product.Description);
            }

            var newProduct = new Product
            {
                Description = product.Description,
                ExternalId = product.ExternalId,
                ReleaseTrack = product.ReleaseTrack,
                ChooseTicketsByDefault = product.ChooseTicketsByDefault,
                BusinessUnitId = businessUnit.BusinessUnitId
            };
            ProductRepository.Insert(newProduct);
        }

        public void UpdateProduct(BusinessEntities.Products.Product product)
        {
            var businessUnit =
                BusinessUnitRepository.GetSatisfiedBy(x => x.ExternalId == product.BusinessUnit.ExternalId);
            if (businessUnit == null)
            {
                throw new EntityNotFoundException(typeof(BusinessUnit), product.BusinessUnit.ExternalId);
            }
            var existingProduct = ProductRepository.GetSatisfiedBy(x => x.ExternalId == product.ExternalId);

            if (existingProduct == null)
            {
                throw new ProductNotFoundException(product.Description);
            }

            existingProduct.Description = product.Description;
            existingProduct.ChooseTicketsByDefault = product.ChooseTicketsByDefault;
            existingProduct.ReleaseTrack = product.ReleaseTrack;
            existingProduct.BusinessUnitId = businessUnit.BusinessUnitId;
            ProductRepository.Update(existingProduct);
        }

        public override void OnDisposing()
        {
            ReleaseWindowRepository.Dispose();
            ProductRepository.Dispose();
            BusinessUnitRepository.Dispose();
            base.OnDisposing();
        }
    }
}
