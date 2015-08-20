using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.CommandHandlers.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using ReMi.Events.ProductRequests;

namespace ReMi.CommandHandlers.Tests.ProductRequests
{
    public class CreateProductRequestRegistrationCommandHandlerTests : TestClassFor<CreateProductRequestRegistrationCommandHandler>
    {
        private Mock<IProductRequestRegistrationGateway> _productRequestRegistrationGatewayMock;
        private Mock<IPublishEvent> _publishEventMock;

        protected override CreateProductRequestRegistrationCommandHandler ConstructSystemUnderTest()
        {
            return new CreateProductRequestRegistrationCommandHandler
            {
                ProductRequestRegistrationGatewayFactory = () => _productRequestRegistrationGatewayMock.Object,
                PublishEvent = _publishEventMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productRequestRegistrationGatewayMock = new Mock<IProductRequestRegistrationGateway>();
            _publishEventMock = new Mock<IPublishEvent>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenInvoked()
        {
            var registration = new ProductRequestRegistration();
            var command = new CreateProductRequestRegistrationCommand
            {
                CommandContext = new CommandContext
                {
                    UserId = Guid.NewGuid()
                },
                Registration = registration
            };

            Sut.Handle(command);

            _productRequestRegistrationGatewayMock.Verify(o => o.CreateProductRequestRegistration(registration), Times.Once);
        }

        [Test]
        public void Handle_ShouldSetChangedByAccountId_WhenInvoked()
        {
            var registration = new ProductRequestRegistration
            {
                CreatedByAccountId = Guid.Empty
            };
            var command = new CreateProductRequestRegistrationCommand
            {
                CommandContext = new CommandContext
                {
                    UserId = Guid.NewGuid()
                },
                Registration = registration
            };

            Sut.Handle(command);

            _productRequestRegistrationGatewayMock.Verify(o =>
                o.CreateProductRequestRegistration(
                    It.Is<ProductRequestRegistration>(x => x.CreatedByAccountId == command.CommandContext.UserId)), Times.Once);
        }

        [Test]
        public void Handle_ShouldSendEvent_WhenInvoked()
        {
            var registration = new ProductRequestRegistration
            {
                CreatedByAccountId = Guid.Empty
            };
            var command = new CreateProductRequestRegistrationCommand
            {
                CommandContext = new CommandContext
                {
                    UserId = Guid.NewGuid()
                },
                Registration = registration
            };

            Sut.Handle(command);

            _publishEventMock.Verify(o =>
                o.Publish(
                    It.Is<ProductRequestRegistrationCreatedEvent>(x => x.Registration == registration)), Times.Once);
        }
    }
}
