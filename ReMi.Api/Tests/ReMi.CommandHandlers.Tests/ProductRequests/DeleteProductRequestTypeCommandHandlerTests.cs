using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.Tests.ProductRequests
{
    public class DeleteProductRequestTypeCommandHandlerTests : TestClassFor<DeleteProductRequestTypeCommandHandler>
    {
        private Mock<IProductRequestGateway> _productRequestGatewayMock;

        protected override DeleteProductRequestTypeCommandHandler ConstructSystemUnderTest()
        {
            return new DeleteProductRequestTypeCommandHandler
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
            var command = new DeleteProductRequestTypeCommand
            {
                RequestTypeId = Guid.NewGuid()
            };

            Sut.Handle(command);

            _productRequestGatewayMock.Verify(o => o.DeleteProductRequestType(command.RequestTypeId));
        }
    }
}
