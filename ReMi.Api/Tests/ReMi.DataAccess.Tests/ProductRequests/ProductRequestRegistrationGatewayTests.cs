using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Constants.ProductRequests;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ProductRequests;

namespace ReMi.DataAccess.Tests.ProductRequests
{
    public class ProductRequestRegistrationGatewayTests : TestClassFor<ProductRequestRegistrationGateway>
    {
        private Mock<IRepository<Account>> _accountRepositoryMock;
        private Mock<IRepository<ProductRequestRegistration>> _productRequestRegistrationRepositoryMock;
        private Mock<IRepository<ProductRequestRegistrationTask>> _productRequestRegistrationTaskRepositoryMock;
        private Mock<IRepository<ProductRequestType>> _productRequestTypeRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override ProductRequestRegistrationGateway ConstructSystemUnderTest()
        {
            return new ProductRequestRegistrationGateway
            {
                AccountRepository = _accountRepositoryMock.Object,
                ProductRequestRegistrationRepository = _productRequestRegistrationRepositoryMock.Object,
                ProductRequestRegistrationTaskRepository = _productRequestRegistrationTaskRepositoryMock.Object,
                ProductRequestTypeRepository = _productRequestTypeRepositoryMock.Object,
                MappingEngine = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountRepositoryMock = new Mock<IRepository<Account>>();
            _productRequestRegistrationRepositoryMock = new Mock<IRepository<ProductRequestRegistration>>();
            _productRequestRegistrationTaskRepositoryMock = new Mock<IRepository<ProductRequestRegistrationTask>>();
            _productRequestTypeRepositoryMock = new Mock<IRepository<ProductRequestType>>();

            _mappingEngineMock = new Mock<IMappingEngine>();
            _mappingEngineMock
                .Setup(o =>
                    o.Map<BusinessEntities.ProductRequests.ProductRequestRegistration, ProductRequestRegistration>(It.IsAny<BusinessEntities.ProductRequests.ProductRequestRegistration>()))
                .Returns<BusinessEntities.ProductRequests.ProductRequestRegistration>(r =>
                    new ProductRequestRegistration
                    {
                        ExternalId = r.ExternalId
                    });

            base.TestInitialize();
        }

        [Test]
        public void GetRegistrations_ShouldThrowException_WhenRegistrationAlreadyExists()
        {
            var entities = new[]
            {
                new ProductRequestRegistration{Deleted = true},
                new ProductRequestRegistration{Deleted = false},
                new ProductRequestRegistration{Deleted = null}
            };
            _productRequestRegistrationRepositoryMock.SetupEntities(entities);

            Sut.GetRegistrations();

            _mappingEngineMock.Verify(x => x.Map<IEnumerable<ProductRequestRegistration>,
                IEnumerable<BusinessEntities.ProductRequests.ProductRequestRegistration>>(
                    It.Is<IEnumerable<ProductRequestRegistration>>(e => e.Count() == 2)), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(EntityAlreadyExistsException))]
        public void CreateProductRequestRegistration_ShouldThrowException_WhenRegistrationAlreadyExists()
        {
            var registrationId = Guid.NewGuid();

            var reg = CreateRegistration(registrationId);

            _productRequestRegistrationRepositoryMock.SetupEntities(new[]
            {
                Builder<ProductRequestRegistration>.CreateNew()
                    .With(o => o.ExternalId, registrationId)
                    .Build()
            });

            Sut.CreateProductRequestRegistration(reg);
        }

        [Test]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void CreateProductRequestRegistration_ShouldThrowException_WhenRequestTypeNotFound()
        {
            var reg = CreateRegistration();

            Sut.CreateProductRequestRegistration(reg);
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void CreateProductRequestRegistration_ShouldThrowException_WhenAccountNotFound()
        {
            var reg = CreateRegistration();

            SetupRequestType(reg.ProductRequestTypeId);

            Sut.CreateProductRequestRegistration(reg);
        }

        [Test]
        public void CreateProductRequestRegistration_ShouldCallMapping_WhenInvoked()
        {
            var reg = CreateRegistration();

            SetupRequestType(reg.ProductRequestTypeId);
            SetupAccount(reg.CreatedByAccountId);

            Sut.CreateProductRequestRegistration(reg);

            _mappingEngineMock.Verify(o => o.Map<BusinessEntities.ProductRequests.ProductRequestRegistration, ProductRequestRegistration>(reg));
        }

        [Test]
        public void CreateProductRequestRegistration_ShouldInsertToRepository_WhenInvoked()
        {
            var reg = CreateRegistration();

            var requestType = SetupRequestType(reg.ProductRequestTypeId);
            var account = SetupAccount(reg.CreatedByAccountId);

            var dt = DateTime.UtcNow;
            SystemTime.Mock(dt);

            Sut.CreateProductRequestRegistration(reg);

            _productRequestRegistrationRepositoryMock.Verify(o =>
                o.Insert(It.Is<ProductRequestRegistration>(x =>
                    x.CreatedOn == dt
                    && x.ProductRequestTypeId == requestType.ProductRequestTypeId
                    && x.CreatedByAccountId == account.AccountId)));
        }

        [Test]
        public void CreateProductRequestRegistration_ShouldInsertChildTasksToRepository_WhenInvoked()
        {
            var taskId = Guid.NewGuid();
            var reg = CreateRegistration(taskId: taskId);

            SetupRequestType(reg.ProductRequestTypeId, taskId);
            var account = SetupAccount(reg.CreatedByAccountId);

            var dt = DateTime.UtcNow;
            SystemTime.Mock(dt);

            var registrationId = RandomData.RandomInt(1, int.MaxValue);
            _productRequestRegistrationRepositoryMock.Setup(o => o.Insert(It.IsAny<ProductRequestRegistration>()))
                .Callback<ProductRequestRegistration>(registration => registration.ProductRequestRegistrationId = registrationId);

            Sut.CreateProductRequestRegistration(reg);

            _productRequestRegistrationTaskRepositoryMock.Verify(o =>
                o.Insert(It.Is<ProductRequestRegistrationTask>(x =>
                    x.LastChangedOn == dt
                    && x.ProductRequestRegistrationId == registrationId
                    && x.LastChangedByAccountId == account.AccountId)));
        }

        [Test]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void UpdateProductRequestRegistration_ShouldThrowException_WhenRegistrationNotFound()
        {
            var registrationId = Guid.NewGuid();

            var reg = CreateRegistration(registrationId);

            Sut.UpdateProductRequestRegistration(reg);
        }

        [Test]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void UpdateProductRequestRegistration_ShouldThrowException_WhenRequestTypeNotFound()
        {
            var reg = CreateRegistration();

            SetupRegistration(reg.ExternalId);

            Sut.UpdateProductRequestRegistration(reg);
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void UpdateProductRequestRegistration_ShouldThrowException_WhenAccountNotFound()
        {
            var reg = CreateRegistration();

            SetupRegistration(reg.ExternalId);

            SetupRequestType(reg.ProductRequestTypeId);

            Sut.UpdateProductRequestRegistration(reg);
        }

        [Test]
        public void UpdateProductRequestRegistration_ShouldInsertToRepository_WhenInvoked()
        {
            var reg = CreateRegistration();

            SetupRegistration(reg.ExternalId);

            SetupRequestType(reg.ProductRequestTypeId);
            SetupAccount(reg.CreatedByAccountId);

            Sut.UpdateProductRequestRegistration(reg);

            _productRequestRegistrationRepositoryMock.Verify(o =>
                o.Update(It.Is<ProductRequestRegistration>(x => x.Description == reg.Description)));
        }

        [Test]
        public void UpdateProductRequestRegistration_ShouldInsertChildTasksToRepository_WhenInvoked()
        {
            var taskId = Guid.NewGuid();
            var reg = CreateRegistration(taskId: taskId);

            SetupRegistration(reg.ExternalId);

            SetupRequestType(reg.ProductRequestTypeId, taskId);
            var account = SetupAccount(reg.CreatedByAccountId);

            var dt = DateTime.UtcNow;
            SystemTime.Mock(dt);

            _productRequestRegistrationTaskRepositoryMock.SetupEntities(new[]
            {
                Builder<ProductRequestRegistrationTask>.CreateNew()
                    .With(o => o.ProductRequestRegistration, Builder<ProductRequestRegistration>.CreateNew().With(x => x.ExternalId, reg.ExternalId).Build())
                    .With(o => o.ProductRequestTask, Builder<ProductRequestTask>.CreateNew().With(x => x.ExternalId, taskId).Build())
                    .Build()
            });

            Sut.UpdateProductRequestRegistration(reg);

            _productRequestRegistrationTaskRepositoryMock.Verify(o =>
                o.Update(It.Is<ProductRequestRegistrationTask>(x =>
                    x.LastChangedOn == dt
                    && x.LastChangedByAccountId == account.AccountId
                    && x.IsCompleted == reg.Tasks.ElementAt(0).IsCompleted)));
        }

        [Test]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void DeleteProductRequestRegistration_ShouldThrowException_WhenRegistrationNotFound()
        {
            var registrationId = Guid.NewGuid();

            var reg = CreateRegistration(registrationId);

            Sut.DeleteProductRequestRegistration(reg.ExternalId, RemovingReason.ClosedAndLive, null);
        }

        [Test]
        public void DeleteProductRequestRegistration_ShouldMarkEntityAsDeletedAndAddRemovingReason_WhenInvoked()
        {
            var registrationId = Guid.NewGuid();
            var removingReason = RandomData.RandomEnum<RemovingReason>();
            var comment = RandomData.RandomString(100);

            var reg = CreateRegistration(registrationId);
            SetupRegistration(registrationId);

            Sut.DeleteProductRequestRegistration(reg.ExternalId, removingReason, comment);

            _productRequestRegistrationRepositoryMock.Verify(o =>
                o.Update(It.Is<ProductRequestRegistration>(x =>
                    x.ExternalId == reg.ExternalId
                    && x.Deleted == true
                    && x.RemovingReason.Comment == comment
                    && x.RemovingReason.RemovingReason == removingReason)));
        }

        [Test]
        public void OnDisposing_ShouldDisposeAllObject_WhenInvoked()
        {
            Sut.OnDisposing();

            _productRequestTypeRepositoryMock.Verify(o => o.Dispose());
            _productRequestRegistrationRepositoryMock.Verify(o => o.Dispose());
            _productRequestRegistrationTaskRepositoryMock.Verify(o => o.Dispose());
            _mappingEngineMock.Verify(o => o.Dispose());
            _accountRepositoryMock.Verify(o => o.Dispose());
        }

        #region Helpers

        private BusinessEntities.ProductRequests.ProductRequestRegistration CreateRegistration(Guid? externalId = null, Guid? taskId = null, bool isCompleted = true)
        {
            return Builder<BusinessEntities.ProductRequests.ProductRequestRegistration>.CreateNew()
                .With(o => o.ExternalId, externalId ?? Guid.NewGuid())
                .With(o => o.CreatedByAccountId, Guid.NewGuid())
                .With(o => o.ProductRequestTypeId, Guid.NewGuid())
                .With(o => o.Tasks,
                    !taskId.HasValue ? null :
                    new[] {
                        Builder<BusinessEntities.ProductRequests.ProductRequestRegistrationTask>.CreateNew()
                            .With(o => o.ProductRequestTaskId, taskId.Value)
                            .With(o => o.IsCompleted, isCompleted)
                            .Build()
                    })
                .Build();
        }

        private ProductRequestRegistration SetupRegistration(Guid? externalId = null, Guid? taskId = null, bool isCompleted = false)
        {
            var registration = Builder<ProductRequestRegistration>.CreateNew()
                .With(o => o.ExternalId, externalId ?? Guid.NewGuid())
                .With(o => o.Description, null)
                .With(x => x.ProductRequestRegistrationId, RandomData.RandomInt(int.MaxValue))
                .With(o => o.Tasks,
                    !taskId.HasValue ? null : new[]{
                        Builder<ProductRequestRegistrationTask>.CreateNew()
                            .With(x => x.ProductRequestTask, Builder<ProductRequestTask>.CreateNew().With(x => x.ExternalId, taskId.Value).Build())
                            .With(x => x.IsCompleted, isCompleted)
                            .Build()
                    }.ToList())
                .Build();

            _productRequestRegistrationRepositoryMock.SetupEntities(new[]
            {
                registration
            });

            return registration;
        }

        private ProductRequestType SetupRequestType(Guid? externalId = null, Guid? taskId = null)
        {
            var result = Builder<ProductRequestType>.CreateNew()
                .With(o => o.ExternalId, externalId ?? Guid.NewGuid())
                .With(o => o.ProductRequestTypeId, RandomData.RandomInt(1, int.MaxValue))
                .With(o => o.RequestGroups, !taskId.HasValue ? null : new[]
                {
                    Builder<ProductRequestGroup>.CreateNew()
                        .With(o => o.ExternalId, Guid.NewGuid())
                        .With(o => o.RequestTasks, 
                            new[]{
                                Builder<ProductRequestTask>.CreateNew()
                                    .With(o => o.ExternalId, taskId.Value)
                                    .With(o => o.Question, RandomData.RandomString(1, 1024))
                                    .Build()
                            })
                        .Build()
                })
                .Build();

            _productRequestTypeRepositoryMock.SetupEntities(new[] { result });

            return result;
        }

        private Account SetupAccount(Guid? externalId = null)
        {
            var result = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, externalId ?? Guid.NewGuid())
                .With(o => o.AccountId, RandomData.RandomInt(1, int.MaxValue))
                .Build();

            _accountRepositoryMock.SetupEntities(new[] { result });

            return result;
        }

        #endregion
    }
}
