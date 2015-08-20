using System;
using System.Collections.Generic;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ProductRequests;

namespace ReMi.DataAccess.BusinessEntityGateways.ProductRequests
{
    public class ProductRequestAssigneeGateway : BaseGateway, IProductRequestAssigneeGateway
    {
        public IRepository<ProductRequestGroupAssignee> ProductRequestGroupAssigneeRepository { get; set; }
        public IRepository<Account> AccountRepository { get; set; }
        public IRepository<ProductRequestGroup> ProductRequestGroupRepository { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public IEnumerable<BusinessEntities.Auth.Account> GetAssignees(Guid requestGroupId)
        {
            var assignees = ProductRequestGroupAssigneeRepository.GetAllSatisfiedBy(o => o.RequestGroup.ExternalId == requestGroupId);

            return
                MappingEngine
                    .Map<IEnumerable<ProductRequestGroupAssignee>, IEnumerable<BusinessEntities.Auth.Account>>(
                        assignees);
        }

        public void AppendAssignee(Guid requestGroupId, Guid assigneeExternalId)
        {
            var dataAccount = AccountRepository.GetSatisfiedBy(o => o.ExternalId == assigneeExternalId);
            if (dataAccount == null)
                throw new AccountNotFoundException(assigneeExternalId);

            var dataRequestGroup = ProductRequestGroupRepository.GetSatisfiedBy(o => o.ExternalId == requestGroupId);
            if (dataRequestGroup == null)
                throw new EntityNotFoundException(typeof(ProductRequestGroup), requestGroupId);

            var existingAssignee = ProductRequestGroupAssigneeRepository.GetSatisfiedBy(o => o.RequestGroup.ExternalId == requestGroupId && o.Account.ExternalId == assigneeExternalId);
            if (existingAssignee == null)
            {
                ProductRequestGroupAssigneeRepository.Insert(
                    new ProductRequestGroupAssignee
                    {
                        AccountId = dataAccount.AccountId,
                        ProductRequestGroupId = dataRequestGroup.ProductRequestGroupId
                    });
            }
        }

        public void RemoveAssignee(Guid requestGroupId, Guid assigneeExternalId)
        {
            var dataAccount = AccountRepository.GetSatisfiedBy(o => o.ExternalId == assigneeExternalId);
            if (dataAccount == null)
                throw new AccountNotFoundException(assigneeExternalId);

            var dataRequestGroup = ProductRequestGroupRepository.GetSatisfiedBy(o => o.ExternalId == requestGroupId);
            if (dataRequestGroup == null)
                throw new EntityNotFoundException(typeof(ProductRequestGroup), requestGroupId);

            var existingAssignee = ProductRequestGroupAssigneeRepository.GetSatisfiedBy(o => o.RequestGroup.ExternalId == requestGroupId && o.Account.ExternalId == assigneeExternalId);
            if (existingAssignee != null)
            {
                ProductRequestGroupAssigneeRepository.Delete(existingAssignee);
            }
        }

        public override void OnDisposing()
        {
            ProductRequestGroupAssigneeRepository.Dispose();
            AccountRepository.Dispose();
            ProductRequestGroupRepository.Dispose();

            base.OnDisposing();
        }
    }
}
