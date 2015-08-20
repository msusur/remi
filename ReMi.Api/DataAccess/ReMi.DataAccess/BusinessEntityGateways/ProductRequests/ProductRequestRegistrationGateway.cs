using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.Common.Constants.ProductRequests;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ProductRequests;
using ProductRequestRegistration = ReMi.BusinessEntities.ProductRequests.ProductRequestRegistration;
using ProductRequestRegistrationTask = ReMi.BusinessEntities.ProductRequests.ProductRequestRegistrationTask;

namespace ReMi.DataAccess.BusinessEntityGateways.ProductRequests
{
    public class ProductRequestRegistrationGateway : BaseGateway, IProductRequestRegistrationGateway
    {
        public IRepository<DataEntities.ProductRequests.ProductRequestRegistration> ProductRequestRegistrationRepository { get; set; }
        public IRepository<DataEntities.ProductRequests.ProductRequestRegistrationTask> ProductRequestRegistrationTaskRepository { get; set; }
        public IRepository<ProductRequestType> ProductRequestTypeRepository { get; set; }
        public IRepository<DataEntities.Auth.Account> AccountRepository { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        private Guid _productRequestTypeId = Guid.Empty;
        private IEnumerable<ProductRequestTask> _tasksByType;

        public ProductRequestRegistration GetRegistration(Guid externalId)
        {
            var existingRegistration = ProductRequestRegistrationRepository.GetSatisfiedBy(o => o.ExternalId == externalId);
            if (existingRegistration == null)
                throw new EntityNotFoundException(typeof(DataEntities.ProductRequests.ProductRequestRegistration), externalId);

            return MappingEngine.Map
                    <DataEntities.ProductRequests.ProductRequestRegistration, ProductRequestRegistration>(existingRegistration);
        }

        public IEnumerable<ProductRequestRegistration> GetRegistrations()
        {
            var items =
                ProductRequestRegistrationRepository.GetAllSatisfiedBy(x => !(x.Deleted.HasValue && x.Deleted.Value));

            return
                MappingEngine
                    .Map
                    <IEnumerable<DataEntities.ProductRequests.ProductRequestRegistration>,
                        IEnumerable<ProductRequestRegistration>>(items);
        }

        public void CreateProductRequestRegistration(ProductRequestRegistration registration)
        {
            var existingRegistration = ProductRequestRegistrationRepository.GetSatisfiedBy(o => o.ExternalId == registration.ExternalId);
            if (existingRegistration != null)
                throw new EntityAlreadyExistsException(typeof(DataEntities.ProductRequests.ProductRequestRegistration), registration.ExternalId);

            var existingRequestType =
                ProductRequestTypeRepository.GetSatisfiedBy(o => o.ExternalId == registration.ProductRequestTypeId);
            if (existingRequestType == null)
                throw new EntityNotFoundException(typeof(ProductRequestType), registration.ProductRequestTypeId);

            var createdByAccount =
                AccountRepository.GetSatisfiedBy(o => o.ExternalId == registration.CreatedByAccountId);
            if (createdByAccount == null)
                throw new AccountNotFoundException(registration.CreatedByAccountId);

            var newEntity =
                MappingEngine.Map<ProductRequestRegistration, DataEntities.ProductRequests.ProductRequestRegistration>(
                    registration);

            newEntity.ProductRequestTypeId = existingRequestType.ProductRequestTypeId;
            newEntity.CreatedOn = SystemTime.Now;
            newEntity.CreatedByAccountId = createdByAccount.AccountId;

            ProductRequestRegistrationRepository.Insert(newEntity);

            UpdateRegistrationTasks(registration, newEntity.ProductRequestRegistrationId, createdByAccount);
        }

        public void UpdateProductRequestRegistration(ProductRequestRegistration registration)
        {
            var existingRegistration = ProductRequestRegistrationRepository.GetSatisfiedBy(o => o.ExternalId == registration.ExternalId);
            if (existingRegistration == null)
                throw new EntityNotFoundException(typeof(DataEntities.ProductRequests.ProductRequestRegistration), registration.ExternalId);

            var existingRequestType =
                ProductRequestTypeRepository.GetSatisfiedBy(o => o.ExternalId == registration.ProductRequestTypeId);
            if (existingRequestType == null)
                throw new EntityNotFoundException(typeof(ProductRequestType), registration.ProductRequestTypeId);

            var createdByAccount =
                AccountRepository.GetSatisfiedBy(o => o.ExternalId == registration.CreatedByAccountId);
            if (createdByAccount == null)
                throw new AccountNotFoundException(registration.CreatedByAccountId);

            if (!string.Equals(existingRegistration.Description, registration.Description, StringComparison.CurrentCulture))
            {
                existingRegistration.Description = registration.Description;
                ProductRequestRegistrationRepository.Update(existingRegistration);
            }

            UpdateRegistrationTasks(registration, existingRegistration.ProductRequestRegistrationId, createdByAccount);
        }

        public void DeleteProductRequestRegistration(Guid productRequestRegistrationId, RemovingReason removingReason, string comment)
        {
            var existingRegistration = ProductRequestRegistrationRepository.GetSatisfiedBy(o => o.ExternalId == productRequestRegistrationId);
            if (existingRegistration == null)
                throw new EntityNotFoundException(typeof(DataEntities.ProductRequests.ProductRequestRegistration), productRequestRegistrationId);

            existingRegistration.RemovingReason = new ProductRequestRegistrationRemovingReason
            {
                ProductRequestRegistrationId = existingRegistration.ProductRequestRegistrationId,
                RemovingReason = removingReason,
                Comment = comment
            };
            existingRegistration.Deleted = true;
            ProductRequestRegistrationRepository.Update(existingRegistration);
        }

        private void UpdateRegistrationTasks(ProductRequestRegistration registration, int parentRegistrationId, DataEntities.Auth.Account currentDataAccount)
        {
            var existingTasks =
                ProductRequestRegistrationTaskRepository
                    .GetAllSatisfiedBy(o => o.ProductRequestRegistration.ExternalId == registration.ExternalId)
                    .ToList();

            if (registration.Tasks != null)
                foreach (var task in registration.Tasks)
                {
                    var existingTask =
                        existingTasks.FirstOrDefault(o => o.ProductRequestTask.ExternalId == task.ProductRequestTaskId);

                    if (existingTask != null)
                    {
                        UpdateExistingTask(existingTask, task, currentDataAccount);
                    }
                    else
                    {
                        CreateNewTask(registration, parentRegistrationId, task, currentDataAccount);
                    }
                }
        }

        private void UpdateExistingTask(DataEntities.ProductRequests.ProductRequestRegistrationTask existingTask, ProductRequestRegistrationTask task, DataEntities.Auth.Account dataAccount)
        {
            SetTaskData(existingTask, task, dataAccount.AccountId);

            ProductRequestRegistrationTaskRepository.Update(existingTask);
        }

        private void CreateNewTask(ProductRequestRegistration registration, int registrationId, ProductRequestRegistrationTask task, DataEntities.Auth.Account dataAccount)
        {
            var taskForType = GetTasksByType(registration.ProductRequestTypeId).FirstOrDefault(o => o.ExternalId == task.ProductRequestTaskId);
            if (taskForType == null)
                throw new EntityNotFoundException(typeof(ProductRequestRegistrationTask), task.ProductRequestTaskId);

            var newEntity =
                new DataEntities.ProductRequests.ProductRequestRegistrationTask
                {
                    ProductRequestTaskId = taskForType.ProductRequestTaskId,
                    ProductRequestRegistrationId = registrationId,
                };

            SetTaskData(newEntity, task, dataAccount.AccountId);

            ProductRequestRegistrationTaskRepository.Insert(newEntity);
        }

        private void SetTaskData(DataEntities.ProductRequests.ProductRequestRegistrationTask dataTask, ProductRequestRegistrationTask businessTask, int accountId)
        {
            if ((dataTask.ProductRequestRegistrationTaskId == 0 &&
                    (businessTask.IsCompleted || !string.IsNullOrWhiteSpace(businessTask.Comment))
                )
                ||
                (dataTask.ProductRequestRegistrationTaskId > 0 &&
                    (businessTask.IsCompleted != dataTask.IsCompleted || !string.Equals(businessTask.Comment, dataTask.Comment, StringComparison.CurrentCulture))
                ))
            {
                dataTask.LastChangedByAccountId = accountId;
                dataTask.LastChangedOn = SystemTime.Now;

                dataTask.IsCompleted = businessTask.IsCompleted;
                dataTask.Comment = businessTask.Comment;
            }
        }

        private IEnumerable<ProductRequestTask> GetTasksByType(Guid productRequestTypeId)
        {
            if (_tasksByType == null || _productRequestTypeId != productRequestTypeId)
            {
                _productRequestTypeId = productRequestTypeId;
                _tasksByType = ProductRequestTypeRepository.GetAllSatisfiedBy(o => o.ExternalId == productRequestTypeId)
                    .SelectMany(o => o.RequestGroups)
                    .SelectMany(o => o.RequestTasks)
                    .ToList();
            }

            return _tasksByType;
        }

        public override void OnDisposing()
        {
            ProductRequestRegistrationRepository.Dispose();
            ProductRequestTypeRepository.Dispose();
            ProductRequestRegistrationTaskRepository.Dispose();
            AccountRepository.Dispose();
            MappingEngine.Dispose();

            base.OnDisposing();
        }
    }
}
