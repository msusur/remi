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
    public class CreateProductRequestTaskCommandHandlerTests : TestClassFor<CreateProductRequestTaskCommandHandler>
    {
        private Mock<IProductRequestGateway> _productRequestGatewayMock;

        protected override CreateProductRequestTaskCommandHandler ConstructSystemUnderTest()
        {
            return new CreateProductRequestTaskCommandHandler
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

            var command = new CreateProductRequestTaskCommand
            {
                RequestTask = task
            };

            Sut.Handle(command);

            _productRequestGatewayMock.Verify(o => o.CreateProductRequestTask(task));
        }
    }
}
