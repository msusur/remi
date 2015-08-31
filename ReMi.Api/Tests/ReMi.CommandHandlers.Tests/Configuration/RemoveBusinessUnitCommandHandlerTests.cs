using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.Configuration;
using ReMi.Commands.Configuration;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Events.Configuration;
using ReMi.TestUtils.UnitTests;
using System;
using System.Threading.Tasks;

namespace ReMi.CommandHandlers.Tests.Configuration
{
    public class RemoveBusinessUnitCommandHandlerTests : TestClassFor<RemoveBusinessUnitCommandHandler>
    {
        private Mock<IBusinessUnitsGateway> _businessUnitsGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override RemoveBusinessUnitCommandHandler ConstructSystemUnderTest()
        {
            return new RemoveBusinessUnitCommandHandler
            {
                BusinessUnitsGatewayFactory = () => _businessUnitsGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _businessUnitsGatewayMock = new Mock<IBusinessUnitsGateway>(MockBehavior.Strict);
            _eventPublisherMock = new Mock<IPublishEvent>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToAddProductAndPublishEvent_WhenCalled()
        {
            var command = new RemoveBusinessUnitCommand
            {
                ExternalId = Guid.NewGuid(),
            };

            _businessUnitsGatewayMock.Setup(x => x.RemoveBusinessUnit(command.ExternalId));
            _eventPublisherMock.Setup(x => x.Publish(It.IsAny<BusinessUnitsChangedEvent>()))
                .Returns((Task[])null);
            _businessUnitsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _businessUnitsGatewayMock.Verify(x => x.RemoveBusinessUnit(It.IsAny<Guid>()), Times.Once);
            _eventPublisherMock.Verify(p => p.Publish(It.IsAny<IEvent>()), Times.Once);
        }
    }
}
