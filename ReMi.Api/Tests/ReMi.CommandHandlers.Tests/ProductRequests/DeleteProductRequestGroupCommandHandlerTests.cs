using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.Tests.ProductRequests
{
    public class DeleteProductRequestGroupCommandHandlerTests : TestClassFor<DeleteProductRequestGroupCommandHandler>
    {
        private Mock<IProductRequestGateway> _productRequestGatewayMock;

        protected override DeleteProductRequestGroupCommandHandler ConstructSystemUnderTest()
        {
            return new DeleteProductRequestGroupCommandHandler
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
            var command = new DeleteProductRequestGroupCommand
            {
                RequestGroupId = Guid.NewGuid()
            };

            Sut.Handle(command);

            _productRequestGatewayMock.Verify(o => o.DeleteProductRequestGroup(command.RequestGroupId));
        }
    }
}
