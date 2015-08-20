using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ProductRequests;

namespace ReMi.DataAccess.BusinessEntityGateways.ProductRequests
{
    public class ProductRequestGateway : BaseGateway, IProductRequestGateway
    {
        public IRepository<ProductRequestType> ProductRequestTypeRepository { get; set; }
        public IRepository<ProductRequestGroup> ProductRequestGroupRepository { get; set; }
        public IRepository<ProductRequestTask> ProductRequestTaskRepository { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public IEnumerable<BusinessEntities.ProductRequests.ProductRequestType> GetRequestTypes()
        {
            return MappingEngine.Map<IEnumerable<ProductRequestType>, IEnumerable<BusinessEntities.ProductRequests.ProductRequestType>>(
                ProductRequestTypeRepository.Entities);
        }

        public BusinessEntities.ProductRequests.ProductRequestType GetRequestType(Guid externalId)
        {
            var type = ProductRequestTypeRepository.GetSatisfiedBy(o => o.ExternalId == externalId);
            if (type == null)
                throw new EntityNotFoundException(typeof(ProductRequestType), externalId);

            return MappingEngine.Map<ProductRequestType, BusinessEntities.ProductRequests.ProductRequestType>(type);
        }

        public void CreateProductRequestType(BusinessEntities.ProductRequests.ProductRequestType type)
        {
            var existing = ProductRequestTypeRepository.GetSatisfiedBy(o => o.ExternalId == type.ExternalId);
            if (existing != null)
                throw new EntityAlreadyExistsException(typeof(ProductRequestType), type.ExternalId);

            var dataType = MappingEngine.Map<BusinessEntities.ProductRequests.ProductRequestType, ProductRequestType>(type);

            ProductRequestTypeRepository.Insert(dataType);
        }

        public void UpdateProductRequestType(BusinessEntities.ProductRequests.ProductRequestType type)
        {
            var existing = ProductRequestTypeRepository.GetSatisfiedBy(o => o.ExternalId == type.ExternalId);
            if (existing == null)
                throw new EntityNotFoundException(typeof(ProductRequestType), type.ExternalId);

            existing.Name = type.Name;

            ProductRequestTypeRepository.Update(existing);
        }

        public void DeleteProductRequestType(Guid productRequestTypeId)
        {
            var existing = ProductRequestTypeRepository.GetSatisfiedBy(o => o.ExternalId == productRequestTypeId);
            if (existing == null)
                throw new EntityNotFoundException("ProductRequestType", productRequestTypeId);

            ProductRequestTypeRepository.Delete(existing);
        }


        public void CreateProductRequestGroup(BusinessEntities.ProductRequests.ProductRequestGroup @group)
        {
            var type = ProductRequestTypeRepository.GetSatisfiedBy(o => o.ExternalId == @group.ProductRequestTypeId);
            if (type == null)
                throw new EntityNotFoundException(typeof(ProductRequestType), @group.ProductRequestTypeId);

            var existing = ProductRequestGroupRepository.GetSatisfiedBy(o => o.ExternalId == @group.ExternalId);
            if (existing != null)
                throw new EntityAlreadyExistsException(typeof(ProductRequestGroup), @group.ExternalId);

            var dataGroup = MappingEngine.Map<BusinessEntities.ProductRequests.ProductRequestGroup, ProductRequestGroup>(@group);

            dataGroup.ProductRequestTypeId = type.ProductRequestTypeId;

            ProductRequestGroupRepository.Insert(dataGroup);
        }

        public void UpdateProductRequestGroup(BusinessEntities.ProductRequests.ProductRequestGroup @group)
        {
            var existing = ProductRequestGroupRepository.GetSatisfiedBy(o => o.ExternalId == @group.ExternalId);
            if (existing == null)
                throw new EntityNotFoundException(typeof(ProductRequestGroup), @group.ExternalId);

            existing.Name = @group.Name;

            ProductRequestGroupRepository.Update(existing);
        }

        public void DeleteProductRequestGroup(Guid productRequestGroupId)
        {
            var existing = ProductRequestGroupRepository.GetSatisfiedBy(o => o.ExternalId == productRequestGroupId);
            if (existing == null)
                throw new EntityNotFoundException(typeof(ProductRequestGroup), productRequestGroupId);

            ProductRequestGroupRepository.Delete(existing);
        }


        public void CreateProductRequestTask(BusinessEntities.ProductRequests.ProductRequestTask task)
        {
            var group = ProductRequestGroupRepository.GetSatisfiedBy(o => o.ExternalId == task.ProductRequestGroupId);
            if (group == null)
                throw new EntityNotFoundException(typeof(ProductRequestGroup), task.ProductRequestGroupId);

            var existing = ProductRequestTaskRepository.GetSatisfiedBy(o => o.ExternalId == task.ExternalId);
            if (existing != null)
                throw new EntityAlreadyExistsException(typeof(ProductRequestTask), task.ExternalId);

            var dataTask = MappingEngine.Map<BusinessEntities.ProductRequests.ProductRequestTask, ProductRequestTask>(task);

            dataTask.ProductRequestGroupId = group.ProductRequestGroupId;

            ProductRequestTaskRepository.Insert(dataTask);
        }

        public void UpdateProductRequestTask(BusinessEntities.ProductRequests.ProductRequestTask task)
        {
            var existing = ProductRequestTaskRepository.GetSatisfiedBy(o => o.ExternalId == task.ExternalId);
            if (existing == null)
                throw new EntityNotFoundException(typeof(ProductRequestTask), task.ExternalId);

            existing.Question = task.Question;

            ProductRequestTaskRepository.Update(existing);
        }

        public void DeleteProductRequestTask(Guid productRequestTaskId)
        {
            var existing = ProductRequestTaskRepository.GetSatisfiedBy(o => o.ExternalId == productRequestTaskId);
            if (existing == null)
                throw new EntityNotFoundException(typeof(ProductRequestTask), productRequestTaskId);

            if (existing.RegistrationTasks != null && existing.RegistrationTasks.Any())
                throw new EntityHasRelatedData(typeof(ProductRequestTask), productRequestTaskId);

            ProductRequestTaskRepository.Delete(existing);
        }

        public IEnumerable<BusinessEntities.ProductRequests.ProductRequestGroup> GetRequestGroupsByTasks(IEnumerable<Guid> taskIds)
        {
            if (taskIds == null)
                throw new ArgumentNullException("taskIds");

            var taskArr = taskIds.ToArray();

            var affectedGroups = ProductRequestTaskRepository.GetAllSatisfiedBy(o => taskArr.Contains(o.ExternalId))
                .Select(o => o.RequestGroup)
                .Distinct()
                .ToList();

            return
                MappingEngine
                    .Map<IEnumerable<ProductRequestGroup>, IEnumerable<BusinessEntities.ProductRequests.ProductRequestGroup>>(affectedGroups);
        }


        public override void OnDisposing()
        {
            ProductRequestTypeRepository.Dispose();
            ProductRequestGroupRepository.Dispose();
            ProductRequestTaskRepository.Dispose();
            MappingEngine.Dispose();

            base.OnDisposing();
        }
    }
}
