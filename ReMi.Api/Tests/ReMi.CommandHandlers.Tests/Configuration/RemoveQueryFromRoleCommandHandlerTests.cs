using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.Configuration;
using ReMi.Commands.Configuration;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Events.Auth;

namespace ReMi.CommandHandlers.Tests.Configuration
{
    public class RemoveQueryFromRoleCommandHandlerTests : TestClassFor<RemoveQueryFromRoleCommandHandler>
    {
        private Mock<IQueryPermissionsGateway> _commandPermissionsGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override RemoveQueryFromRoleCommandHandler ConstructSystemUnderTest()
        {
            return new RemoveQueryFromRoleCommandHandler
            {
                QueryPermissionsGatewayFactory = () => _commandPermissionsGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandPermissionsGatewayMock = new Mock<IQueryPermissionsGateway>(MockBehavior.Strict);
            _eventPublisherMock = new Mock<IPublishEvent>();
            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallDataGateway_WhenInvoked()
        {
            var command = new RemoveQueryFromRoleCommand
            {
                QueryId = RandomData.RandomInt(10000),
                RoleExternalId = Guid.NewGuid()
            };
            _commandPermissionsGatewayMock.Setup(x => x.RemoveQueryPermission(command.QueryId, command.RoleExternalId));
            _commandPermissionsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _commandPermissionsGatewayMock.Verify(x => x.RemoveQueryPermission(command.QueryId, command.RoleExternalId), Times.Once());
            _eventPublisherMock.Verify(
                e => e.Publish(It.Is<PermissionsUpdatedEvent>(x => x.RoleId == command.RoleExternalId)));
        }
    }
}
