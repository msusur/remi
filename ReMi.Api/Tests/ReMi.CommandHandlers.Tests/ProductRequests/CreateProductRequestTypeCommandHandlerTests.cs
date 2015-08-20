using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.CommandHandlers.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.Tests.ProductRequests
{
    public class CreateProductRequestTypeCommandHandlerTests : TestClassFor<CreateProductRequestTypeCommandHandler>
    {
        private Mock<IProductRequestGateway> _productRequestGatewayMock;

        protected override CreateProductRequestTypeCommandHandler ConstructSystemUnderTest()
        {
            return new CreateProductRequestTypeCommandHandler
            {
                ProductRequestGatewayFactory = () => _productRequestGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productRequestGatewayMock = new Mock<IProductRequestGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenInvoked()
        {
            var type = Builder<ProductRequestType>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            var command = new CreateProductRequestTypeCommand
            {
                RequestType = type
            };

            Sut.Handle(command);

            _productRequestGatewayMock.Verify(o => o.CreateProductRequestType(type));
        }
    }
}
