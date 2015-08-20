using System;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Products;
using ReMi.CommandHandlers.Configuration;
using ReMi.Commands.Configuration;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Events.Configuration;

namespace ReMi.CommandHandlers.Tests.Configuration
{
    public class UpdateProductHandlerTests : TestClassFor<UpdateProductHandler>
    {
        private Mock<IProductGateway> _productGatewayMock;
        private Mock<IMappingEngine> _mappingEngineMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override UpdateProductHandler ConstructSystemUnderTest()
        {
            return new UpdateProductHandler
            {
                ProductGatewayFactory = () => _productGatewayMock.Object,
                MappingEngine = _mappingEngineMock.Object,
                EventPublisher = _eventPublisherMock.Object,
            };
        }

        protected override void TestInitialize()
        {
            _productGatewayMock = new Mock<IProductGateway>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);
            _eventPublisherMock = new Mock<IPublishEvent>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldUpdateProductAndPublishEvent_WhenCalled()
        {
            var product = new Product
            {
                Description = RandomData.RandomString(5),
                ExternalId = Guid.NewGuid(),
                BusinessUnit = new BusinessUnit { ExternalId = Guid.NewGuid() },
                ChooseTicketsByDefault = RandomData.RandomBool()
            };
            var command = new UpdateProductCommand
            {
                Description = product.Description,
                ExternalId = product.ExternalId,
                BusinessUnitId = product.BusinessUnit.ExternalId,
                ChooseTicketsByDefault = product.ChooseTicketsByDefault,
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            };

            _mappingEngineMock.Setup(x => x.Map<UpdateProductCommand, Product>(command))
                .Returns(product);
            _productGatewayMock.Setup(x => x.UpdateProduct(product));
            _eventPublisherMock.Setup(x => x.Publish(It.IsAny<BusinessUnitsChangedEvent>()))
                .Returns((Task[])null);
            _productGatewayMock.Setup(p => p.Dispose());

            Sut.Handle(command);

            _productGatewayMock.Verify(p => p.UpdateProduct(It.IsAny<Product>()), Times.Once);
            _eventPublisherMock.Verify(p => p.Publish(It.IsAny<IEvent>()), Times.Once);
            _productGatewayMock.Verify(p => p.Dispose(), Times.Once);
        }
    }
}
