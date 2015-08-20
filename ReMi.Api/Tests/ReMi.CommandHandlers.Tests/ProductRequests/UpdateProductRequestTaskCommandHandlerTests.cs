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
    public class UpdateProductRequestTaskCommandHandlerTests : TestClassFor<UpdateProductRequestTaskCommandHandler>
    {
        private Mock<IProductRequestGateway> _productRequestGatewayMock;

        protected override UpdateProductRequestTaskCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateProductRequestTaskCommandHandler
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
            var task = Builder<ProductRequestTask>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            var command = new UpdateProductRequestTaskCommand
            {
                RequestTask = task
            };

            Sut.Handle(command);

            _productRequestGatewayMock.Verify(o => o.UpdateProductRequestTask(task));
        }
    }
}
