using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.Tests.ProductRequests
{
    public class DeleteProductRequestTaskCommandHandlerTests : TestClassFor<DeleteProductRequestTaskCommandHandler>
    {
        private Mock<IProductRequestGateway> _productRequestGatewayMock;

        protected override DeleteProductRequestTaskCommandHandler ConstructSystemUnderTest()
        {
            return new DeleteProductRequestTaskCommandHandler
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
            var command = new DeleteProductRequestTaskCommand
            {
                RequestTaskId = Guid.NewGuid()
            };

            Sut.Handle(command);

            _productRequestGatewayMock.Verify(o => o.DeleteProductRequestTask(command.RequestTaskId));
        }
    }
}
