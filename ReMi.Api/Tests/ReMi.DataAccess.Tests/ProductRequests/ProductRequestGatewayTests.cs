using System;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ProductRequests;

namespace ReMi.DataAccess.Tests.ProductRequests
{
    public class ProductRequestGatewayTests : TestClassFor<ProductRequestGateway>
    {
        private Mock<IRepository<ProductRequestType>> _productRequestTypeRepositoryMock;
        private Mock<IRepository<ProductRequestGroup>> _productRequestGroupRepositoryMock;
        private Mock<IRepository<ProductRequestTask>> _productRequestTaskRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override ProductRequestGateway ConstructSystemUnderTest()
        {
            return new ProductRequestGateway
            {
                ProductRequestTypeRepository = _productRequestTypeRepositoryMock.Object,
                ProductRequestGroupRepository = _productRequestGroupRepositoryMock.Object,
                ProductRequestTaskRepository = _productRequestTaskRepositoryMock.Object,
                MappingEngine = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productRequestTypeRepositoryMock = new Mock<IRepository<ProductRequestType>>();
            _productRequestGroupRepositoryMock = new Mock<IRepository<ProductRequestGroup>>();
            _productRequestTaskRepositoryMock = new Mock<IRepository<ProductRequestTask>>();

            _mappingEngineMock = new Mock<IMappingEngine>();
            _mappingEngineMock
                .Setup(m => m.Map<BusinessEntities.ProductRequests.ProductRequestType, ProductRequestType>(It.IsAny<BusinessEntities.ProductRequests.ProductRequestType>()))
                .Returns<BusinessEntities.ProductRequests.ProductRequestType>(r => new ProductRequestType
                {
                    ExternalId = r.ExternalId,
                    Name = r.Name
                });

            _mappingEngineMock
                .Setup(m => m.Map<BusinessEntities.ProductRequests.ProductRequestGroup, ProductRequestGroup>(It.IsAny<BusinessEntities.ProductRequests.ProductRequestGroup>()))
                .Returns<BusinessEntities.ProductRequests.ProductRequestGroup>(r => new ProductRequestGroup
                {
                    ExternalId = r.ExternalId,
                    Name = r.Name
                });

            _mappingEngineMock
                .Setup(m => m.Map<BusinessEntities.ProductRequests.ProductRequestTask, ProductRequestTask>(It.IsAny<BusinessEntities.ProductRequests.ProductRequestTask>()))
                .Returns<BusinessEntities.ProductRequests.ProductRequestTask>(r => new ProductRequestTask
                {
                    ExternalId = r.ExternalId,
                    Question = r.Question
                });

            base.TestInitialize();
        }

        [Test]
        public void GetRequestTypes_ShouldGetEntities_WhenInvoked()
        {
            Sut.GetRequestTypes();

            _productRequestTypeRepositoryMock.VerifyGet(o => o.Entities);

        }


        [Test]
        [ExpectedException(typeof(EntityAlreadyExistsException), ExpectedMessage = "Entity '2d9a05af-605e-42d5-a1b0-5403306f3069' with type 'ProductRequestType' already exists")]
        public void CreateProductRequestType_ShouldThrowException_WhenExternalIdAlreadyExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");

            var type = Builder<BusinessEntities.ProductRequests.ProductRequestType>.CreateNew()
                .With(o => o.ExternalId, id)
                .Build();

            SetupProductRequestTypeRepository(id);

            Sut.CreateProductRequestType(type);
        }

        [Test]
        public void CreateProductRequestType_ShouldMapInputData_WhenInvoked()
        {
            var type = Builder<BusinessEntities.ProductRequests.ProductRequestType>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            Sut.CreateProductRequestType(type);

            _mappingEngineMock.Verify(o => o.Map<BusinessEntities.ProductRequests.ProductRequestType, ProductRequestType>(type));
        }

        [Test]
        public void CreateProductRequestType_ShouldInsertNewRecord_WhenInvoked()
        {
            var type = Builder<BusinessEntities.ProductRequests.ProductRequestType>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            Sut.CreateProductRequestType(type);

            _productRequestTypeRepositoryMock.Verify(o =>
                o.Insert(It.Is<ProductRequestType>(x =>
                    x.ExternalId == type.ExternalId)));
        }


        [Test]
        [ExpectedException(typeof(EntityNotFoundException), ExpectedMessage = "Could not find entity '2d9a05af-605e-42d5-a1b0-5403306f3069' of type 'ProductRequestType'")]
        public void UpdateProductRequestType_ShouldThrowException_WhenExternalIdAlreadyExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");

            var type = Builder<BusinessEntities.ProductRequests.ProductRequestType>.CreateNew()
                .With(o => o.ExternalId, id)
                .Build();

            Sut.UpdateProductRequestType(type);
        }

        [Test]
        public void UpdateProductRequestType_ShouldInsertNewRecord_WhenInvoked()
        {
            var type = Builder<BusinessEntities.ProductRequests.ProductRequestType>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            SetupProductRequestTypeRepository(type.ExternalId);

            var newName = RandomData.RandomString(1, 100);

            Sut.UpdateProductRequestType(new BusinessEntities.ProductRequests.ProductRequestType
            {
                ExternalId = type.ExternalId,
                Name = newName
            });

            _productRequestTypeRepositoryMock.Verify(o =>
                o.Update(It.Is<ProductRequestType>(x =>
                    x.ExternalId == type.ExternalId
                    && x.Name == newName)));
        }
        
        [Test]
        [ExpectedException(typeof(EntityNotFoundException), ExpectedMessage = "Could not find entity '2d9a05af-605e-42d5-a1b0-5403306f3069' of type 'ProductRequestType'")]
        public void DeleteProductRequestType_ShouldThrowException_WhenExternalIdNotExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");

            var type = Builder<BusinessEntities.ProductRequests.ProductRequestType>.CreateNew()
                .With(o => o.ExternalId, id)
                .Build();

            Sut.DeleteProductRequestType(type.ExternalId);
        }

        [Test]
        public void DeleteProductRequestType_ShouldInsertNewRecord_WhenInvoked()
        {
            var type = Builder<BusinessEntities.ProductRequests.ProductRequestType>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            SetupProductRequestTypeRepository(type.ExternalId);

            Sut.DeleteProductRequestType(type.ExternalId);

            _productRequestTypeRepositoryMock.Verify(o =>
                o.Delete(It.Is<ProductRequestType>(x =>
                    x.ExternalId == type.ExternalId)));
        }


        [Test]
        [ExpectedException(typeof(EntityNotFoundException), ExpectedMessage = "Could not find entity '2d9a05af-605e-42d5-a1b0-5403306f3069' of type 'ProductRequestType'")]
        public void CreateProductRequestGroup_ShouldThrowException_WhenRelatedTypeNotExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");

            var type = Builder<BusinessEntities.ProductRequests.ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ProductRequestTypeId, id)
                .Build();

            Sut.CreateProductRequestGroup(type);
        }

        [Test]
        [ExpectedException(typeof(EntityAlreadyExistsException), ExpectedMessage = "Entity '2d9a05af-605e-42d5-a1b0-5403306f3069' with type 'ProductRequestGroup' already exists")]
        public void CreateProductRequestGroup_ShouldThrowException_WhenExternalIdAlreadyExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");
            var typeId = Guid.Parse("{922E2940-6F6B-4406-B7BC-4A0FD89F112E}");

            var group = Builder<BusinessEntities.ProductRequests.ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, id)
                .With(o => o.ProductRequestTypeId, typeId)
                .Build();

            SetupProductRequestTypeRepository(typeId);

            SetupProductRequestGroupRepository(id);

            Sut.CreateProductRequestGroup(group);
        }

        [Test]
        public void CreateProductRequestGroup_ShouldMapInputData_WhenInvoked()
        {
            var typeId = Guid.NewGuid();

            var group = Builder<BusinessEntities.ProductRequests.ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ProductRequestTypeId, typeId)
                .Build();

            SetupProductRequestTypeRepository(typeId);

            Sut.CreateProductRequestGroup(group);

            _mappingEngineMock.Verify(o => o.Map<BusinessEntities.ProductRequests.ProductRequestGroup, ProductRequestGroup>(group));
        }

        [Test]
        public void CreateProductRequestGroup_ShouldInsertNewRecord_WhenInvoked()
        {
            var typeExternalId = Guid.NewGuid();
            var typeId = RandomData.RandomInt(1, int.MaxValue);

            var group = Builder<BusinessEntities.ProductRequests.ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ProductRequestTypeId, typeExternalId)
                .Build();

            SetupProductRequestTypeRepository(typeExternalId, typeId);

            Sut.CreateProductRequestGroup(group);

            _productRequestGroupRepositoryMock.Verify(o =>
                o.Insert(It.Is<ProductRequestGroup>(x =>
                    x.ExternalId == group.ExternalId
                    && x.ProductRequestTypeId == typeId)));
        }

        [Test]
        [ExpectedException(typeof(EntityNotFoundException), ExpectedMessage = "Could not find entity '2d9a05af-605e-42d5-a1b0-5403306f3069' of type 'ProductRequestGroup'")]
        public void UpdateProductRequestGroup_ShouldThrowException_WhenExternalIdAlreadyExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");

            var group = Builder<BusinessEntities.ProductRequests.ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, id)
                .Build();

            Sut.UpdateProductRequestGroup(group);
        }

        [Test]
        public void UpdateProductRequestGroup_ShouldInsertNewRecord_WhenInvoked()
        {
            var group = Builder<BusinessEntities.ProductRequests.ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            SetupProductRequestGroupRepository(group.ExternalId);

            var newName = RandomData.RandomString(1, 100);

            Sut.UpdateProductRequestGroup(new BusinessEntities.ProductRequests.ProductRequestGroup
            {
                ExternalId = group.ExternalId,
                Name = newName
            });

            _productRequestGroupRepositoryMock.Verify(o =>
                o.Update(It.Is<ProductRequestGroup>(x =>
                    x.ExternalId == group.ExternalId
                    && x.Name == newName)));
        }

        [Test]
        [ExpectedException(typeof(EntityNotFoundException), ExpectedMessage = "Could not find entity '2d9a05af-605e-42d5-a1b0-5403306f3069' of type 'ProductRequestGroup'")]
        public void DeleteProductRequestGroup_ShouldThrowException_WhenRelatedTypeNotExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");

            var group = Builder<BusinessEntities.ProductRequests.ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, id )
                .With(o => o.ProductRequestTypeId, Guid.NewGuid())
                .Build();

            Sut.DeleteProductRequestGroup(group.ExternalId);
        }

        [Test]
        public void DeleteProductRequestGroup_ShouldInsertNewRecord_WhenInvoked()
        {
            var typeExternalId = Guid.NewGuid();
            var typeId = RandomData.RandomInt(1, int.MaxValue);

            var group = Builder<BusinessEntities.ProductRequests.ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ProductRequestTypeId, typeExternalId)
                .Build();

            SetupProductRequestTypeRepository(typeExternalId, typeId);

            SetupProductRequestGroupRepository(group.ExternalId);

            Sut.DeleteProductRequestGroup(group.ExternalId);

            _productRequestGroupRepositoryMock.Verify(o =>
                o.Delete(It.Is<ProductRequestGroup>(x =>
                    x.ExternalId == group.ExternalId)));
        }


        [Test]
        [ExpectedException(typeof(EntityNotFoundException), ExpectedMessage = "Could not find entity '2d9a05af-605e-42d5-a1b0-5403306f3069' of type 'ProductRequestGroup'")]
        public void CreateProductRequestTask_ShouldThrowException_WhenRelatedTypeNotExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");

            var task = Builder<BusinessEntities.ProductRequests.ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ProductRequestGroupId, id)
                .Build();

            Sut.CreateProductRequestTask(task);
        }

        [Test]
        [ExpectedException(typeof(EntityAlreadyExistsException), ExpectedMessage = "Entity '2d9a05af-605e-42d5-a1b0-5403306f3069' with type 'ProductRequestTask' already exists")]
        public void CreateProductRequestTask_ShouldThrowException_WhenExternalIdAlreadyExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");
            var groupId = Guid.Parse("{922E2940-6F6B-4406-B7BC-4A0FD89F112E}");

            var task = Builder<BusinessEntities.ProductRequests.ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, id)
                .With(o => o.ProductRequestGroupId, groupId)
                .Build();

            SetupProductRequestGroupRepository(groupId);

            SetupProductRequestTaskRepository(id);

            Sut.CreateProductRequestTask(task);
        }

        [Test]
        public void CreateProductRequestTask_ShouldMapInputData_WhenInvoked()
        {
            var groupId = Guid.NewGuid();

            var task = Builder<BusinessEntities.ProductRequests.ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ProductRequestGroupId, groupId)
                .Build();

            SetupProductRequestGroupRepository(groupId);

            Sut.CreateProductRequestTask(task);

            _mappingEngineMock.Verify(o => o.Map<BusinessEntities.ProductRequests.ProductRequestTask, ProductRequestTask>(task));
        }

        [Test]
        public void CreateProductRequestTask_ShouldInsertNewRecord_WhenInvoked()
        {
            var groupId = Guid.NewGuid();

            var task = Builder<BusinessEntities.ProductRequests.ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ProductRequestGroupId, groupId)
                .Build();

            SetupProductRequestGroupRepository(groupId);

            Sut.CreateProductRequestTask(task);

            _productRequestTaskRepositoryMock.Verify(o =>
                o.Insert(It.Is<ProductRequestTask>(x =>
                    x.ExternalId == task.ExternalId)));
        }

        [Test]
        [ExpectedException(typeof(EntityNotFoundException), ExpectedMessage = "Could not find entity '2d9a05af-605e-42d5-a1b0-5403306f3069' of type 'ProductRequestTask'")]
        public void UpdateProductRequestTask_ShouldThrowException_WhenExternalIdAlreadyExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");

            var task = Builder<BusinessEntities.ProductRequests.ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, id)
                .Build();

            Sut.UpdateProductRequestTask(task);
        }

        [Test]
        public void UpdateProductRequestTask_ShouldInsertNewRecord_WhenInvoked()
        {
            var task = Builder<BusinessEntities.ProductRequests.ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            SetupProductRequestTaskRepository(task.ExternalId);

            var newQuestion = RandomData.RandomString(1, 100);

            Sut.UpdateProductRequestTask(new BusinessEntities.ProductRequests.ProductRequestTask
            {
                ExternalId = task.ExternalId,
                Question = newQuestion
            });

            _productRequestTaskRepositoryMock.Verify(o =>
                o.Update(It.Is<ProductRequestTask>(x =>
                    x.ExternalId == task.ExternalId
                    && x.Question == newQuestion)));
        }

        [Test]
        [ExpectedException(typeof(EntityNotFoundException), ExpectedMessage = "Could not find entity '2d9a05af-605e-42d5-a1b0-5403306f3069' of type 'ProductRequestTask'")]
        public void DeleteProductRequestTask_ShouldThrowException_WhenRelatedTypeNotExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");

            var task = Builder<BusinessEntities.ProductRequests.ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, id)
                .Build();

            Sut.DeleteProductRequestTask(task.ExternalId);
        }

        [Test]
        [ExpectedException(typeof(EntityHasRelatedData), ExpectedMessage = "Entity '2d9a05af-605e-42d5-a1b0-5403306f3069' with type 'ProductRequestTask' has related data")]
        public void DeleteProductRequestTask_ShouldThrowException_WhenRelatedRegistrationsNotExists()
        {
            var id = Guid.Parse("{2D9A05AF-605E-42D5-A1B0-5403306F3069}");

            var task = Builder<BusinessEntities.ProductRequests.ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, id)
                .Build();

            SetupProductRequestTaskRepository(task.ExternalId, true);

            Sut.DeleteProductRequestTask(task.ExternalId);
        }

        [Test]
        public void DeleteProductRequestTask_ShouldInsertNewRecord_WhenInvoked()
        {
            var taskId = Guid.NewGuid();

            var task = Builder<BusinessEntities.ProductRequests.ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, taskId)
                .Build();

            SetupProductRequestTaskRepository(task.ExternalId);

            Sut.DeleteProductRequestTask(task.ExternalId);

            _productRequestTaskRepositoryMock.Verify(o =>
                o.Delete(It.Is<ProductRequestTask>(x =>
                    x.ExternalId == task.ExternalId)));
        }


        [Test]
        public void Dispose_ShouldDisposeAllInternalObjects_WhenInvoked()
        {
            Sut.OnDisposing();

            _productRequestTaskRepositoryMock.Verify(o => o.Dispose());
            _productRequestTypeRepositoryMock.Verify(o => o.Dispose());
            _productRequestGroupRepositoryMock.Verify(o => o.Dispose());
            _mappingEngineMock.Verify(o => o.Dispose());
        }

        private void SetupProductRequestTypeRepository(Guid? externalId, int? id = null)
        {
            var type = Builder<ProductRequestType>.CreateNew()
                .With(o => o.ExternalId, externalId ?? Guid.NewGuid())
                .With(o => o.ProductRequestTypeId, id ?? RandomData.RandomInt(1, int.MaxValue))
                .Build();

            _productRequestTypeRepositoryMock.SetupEntities(new[] { type });
        }

        private void SetupProductRequestGroupRepository(Guid? externalId)
        {
            var group = Builder<ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, externalId ?? Guid.NewGuid())
                .Build();

            _productRequestGroupRepositoryMock.SetupEntities(new[] { group });
        }

        private void SetupProductRequestTaskRepository(Guid? externalId, bool relatedRegistrations = false)
        {
            var task = Builder<ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, externalId ?? Guid.NewGuid())
                .With(o => o.RegistrationTasks, !relatedRegistrations ? null : Builder<ProductRequestRegistrationTask>.CreateListOfSize(1).Build())
                .Build();

            _productRequestTaskRepositoryMock.SetupEntities(new[] { task });
        }
    }
}
