using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Products;
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
    public class AddBusinessUnitCommandHandlerTests : TestClassFor<AddBusinessUnitCommandHandler>
    {
        private Mock<IBusinessUnitsGateway> _businessUnitsGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override AddBusinessUnitCommandHandler ConstructSystemUnderTest()
        {
            return new AddBusinessUnitCommandHandler
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
            var command = new AddBusinessUnitCommand
            {
                Description = RandomData.RandomString(20),
                ExternalId = Guid.NewGuid(),
                Name = RandomData.RandomString(10)
            };

            _businessUnitsGatewayMock.Setup(x => x.AddBusinessUnit(It.Is<BusinessUnit>(bu =>
                bu.ExternalId == command.ExternalId
                && bu.Name == command.Name
                && bu.Description == command.Description)));
            _eventPublisherMock.Setup(x => x.Publish(It.IsAny<BusinessUnitsChangedEvent>()))
                .Returns((Task[])null);
            _businessUnitsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _businessUnitsGatewayMock.Verify(x => x.AddBusinessUnit(It.IsAny<BusinessUnit>()), Times.Once);
            _eventPublisherMock.Verify(p => p.Publish(It.IsAny<IEvent>()), Times.Once);
        }
    }
}
