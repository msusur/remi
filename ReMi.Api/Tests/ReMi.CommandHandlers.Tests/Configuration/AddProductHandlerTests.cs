using System;
using System.Collections.Generic;
using System.Linq;
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
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Events.Configuration;
using ReMi.Events.Packages;

namespace ReMi.CommandHandlers.Tests.Configuration
{
    public class AddProductHandlerTests : TestClassFor<AddProductHandler>
    {
        private Mock<IProductGateway> _productGatewayMock;
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<IMappingEngine> _mappingEngineMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override AddProductHandler ConstructSystemUnderTest()
        {
            return new AddProductHandler
            {
                ProductGatewayFactory = () => _productGatewayMock.Object,
                AccountsGateway = () => _accountsGatewayMock.Object,
                MappingEngine = _mappingEngineMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productGatewayMock = new Mock<IProductGateway>(MockBehavior.Strict);
            _accountsGatewayMock = new Mock<IAccountsGateway>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);
            _eventPublisherMock = new Mock<IPublishEvent>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToAddProductAndPublishEvent_WhenCalled()
        {
            var product = new Product
            {
                Description = RandomData.RandomString(5),
                ExternalId = Guid.NewGuid(),
                BusinessUnit = new BusinessUnit { ExternalId = Guid.NewGuid() },
                ChooseTicketsByDefault = RandomData.RandomBool()
            };
            var command = new AddProductCommand
            {
                Description = product.Description,
                ExternalId = product.ExternalId,
                BusinessUnitId = product.BusinessUnit.ExternalId,
                ChooseTicketsByDefault = product.ChooseTicketsByDefault,
                CommandContext = new CommandContext
                {
                    UserId = Guid.NewGuid(),
                    Id = new Guid()
                }
            };

            _mappingEngineMock.Setup(x => x.Map<AddProductCommand, Product>(command))
                .Returns(product);
            _productGatewayMock.Setup(p => p.Dispose());
            _accountsGatewayMock.Setup(p => p.Dispose());
            _eventPublisherMock.Setup(x => x.Publish(It.IsAny<BusinessUnitsChangedEvent>()))
                .Returns((Task[])null);
            _eventPublisherMock.Setup(x => x.Publish(It.Is<NewPackageAddedEvent>(
                e => e.Package == product && e.Context.ParentId == command.CommandContext.Id)))
                .Returns((Task[])null);
            _productGatewayMock.Setup(p => p.Dispose());
            _productGatewayMock.Setup(p => p.AddProduct(product));
            _accountsGatewayMock.Setup(p =>
                p.AssociateAccountsWithProducts(new[] { command.ExternalId },
                    It.Is<IEnumerable<string>>(x => x.First() == command.CommandContext.UserEmail)));

            Sut.Handle(command);

            _productGatewayMock.Verify(p => p.AddProduct(It.IsAny<Product>()));
            _accountsGatewayMock.Verify(
                p =>
                    p.AssociateAccountsWithProducts(It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<string>>()));
            _eventPublisherMock.Verify(p => p.Publish(It.IsAny<IEvent>()), Times.Exactly(2));

            _productGatewayMock.Verify(p => p.Dispose(), Times.Once);
            _accountsGatewayMock.Verify(p => p.Dispose(), Times.Once);
        }
    }
}
