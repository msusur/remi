using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using BusinessUnit = ReMi.BusinessEntities.Products.BusinessUnit;
using Product = ReMi.DataEntities.Products.Product;

namespace ReMi.DataAccess.BusinessEntityGateways.Products
{
    public class BusinessUnitsGateway : BaseGateway, IBusinessUnitsGateway
    {
        public IRepository<DataEntities.Products.BusinessUnit> BusinessUnitRepository { get; set; }
        public IRepository<Product> PackageRepository { get; set; }
        public IRepository<Account> AccountRepository { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public IEnumerable<BusinessEntities.Products.Product> GetPackages(Guid accountId, bool includeAll = false)
        {
            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == accountId);
            if (account == null)
                throw new AccountNotFoundException(accountId);

            var packages = includeAll || account.Role.Name == "Admin"
                    ? PackageRepository.Entities
                    : PackageRepository.GetAllSatisfiedBy(x => x.AccountProducts.Any(a => a.AccountId == account.AccountId));

            return MappingEngine.Map<IEnumerable<Product>, IEnumerable<BusinessEntities.Products.Product>>(packages);
        }

        public BusinessEntities.Products.Product GetDefaultPackage(Guid accountId)
        {
            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == accountId);
            if (account == null)
                throw new AccountNotFoundException(accountId);

            var defaultAccountPackage = account.AccountProducts.FirstOrDefault(a => a.IsDefault) ?? account.AccountProducts.FirstOrDefault();
            return defaultAccountPackage == null
                ? null
                : MappingEngine.Map<Product, BusinessEntities.Products.Product>(defaultAccountPackage.Product);
        }

        public IEnumerable<BusinessUnit> GetEmptyBusinessUnits()
        {
            var businessUnits = BusinessUnitRepository.GetAllSatisfiedBy(
                x => !x.Packages.Any()) ?? Enumerable.Empty<DataEntities.Products.BusinessUnit>();
            return MappingEngine.Map<IEnumerable<DataEntities.Products.BusinessUnit>, IEnumerable<BusinessUnit>>(businessUnits);
        }

        public void AddBusinessUnit(BusinessUnit businessUnit)
        {
            if (BusinessUnitRepository.Entities.Any(x => x.ExternalId == businessUnit.ExternalId))
                throw new EntityAlreadyExistsException(typeof(DataEntities.Products.BusinessUnit), businessUnit.ExternalId);

            BusinessUnitRepository.Insert(
                MappingEngine.Map<BusinessUnit, DataEntities.Products.BusinessUnit>(businessUnit));
        }

        public void UpdateBusinessUnit(BusinessUnit businessUnit)
        {
            var bu = BusinessUnitRepository.GetSatisfiedBy(x => x.ExternalId == businessUnit.ExternalId);
            if (bu == null)
                throw new EntityNotFoundException(typeof(DataEntities.Products.BusinessUnit), businessUnit.ExternalId);

            bu.Description = businessUnit.Description;
            bu.Name = businessUnit.Name;

            BusinessUnitRepository.Update(bu);
        }

        public void RemoveBusinessUnit(Guid businessUnitId)
        {
            var bu = BusinessUnitRepository.GetSatisfiedBy(x => x.ExternalId == businessUnitId);
            if (bu == null)
                throw new EntityNotFoundException(typeof(DataEntities.Products.BusinessUnit), businessUnitId);
            if (bu.Packages.Any())
                throw new EntityHasRelatedData(typeof(DataEntities.Products.BusinessUnit), businessUnitId);

            BusinessUnitRepository.Delete(bu);
        }

        public override void OnDisposing()
        {
            PackageRepository.Dispose();
            AccountRepository.Dispose();
            BusinessUnitRepository.Dispose();
        }
    }
}
