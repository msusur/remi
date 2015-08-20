using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.Common.Constants.ProductRequests;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using System;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandHandlers.Tests.ProductRequests
{
    public class DeleteProductRequestRegistrationCommandHandlerTests : TestClassFor<DeleteProductRequestRegistrationCommandHandler>
    {
        private Mock<IProductRequestRegistrationGateway> _productRequestRegistrationGatewayMock;

        protected override DeleteProductRequestRegistrationCommandHandler ConstructSystemUnderTest()
        {
            return new DeleteProductRequestRegistrationCommandHandler
            {
                ProductRequestRegistrationGatewayFactory = () => _productRequestRegistrationGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productRequestRegistrationGatewayMock = new Mock<IProductRequestRegistrationGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenInvoked()
        {
            var command = new DeleteProductRequestRegistrationCommand
            {
                RegistrationId = Guid.NewGuid(),
				RemovingReason = RandomData.RandomEnum<RemovingReason>(),
				Comment = RandomData.RandomString(100)
            };

            Sut.Handle(command);

            _productRequestRegistrationGatewayMock.Verify(o => o.DeleteProductRequestRegistration(
                command.RegistrationId, command.RemovingReason, command.Comment), Times.Once);
			_productRequestRegistrationGatewayMock.Verify(o => o.Dispose(), Times.Once);
        }
    }
}
