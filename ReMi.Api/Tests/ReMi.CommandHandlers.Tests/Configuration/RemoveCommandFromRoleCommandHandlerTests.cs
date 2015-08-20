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
    public class RemoveCommandFromRoleCommandHandlerTests : TestClassFor<RemoveCommandFromRoleCommandHandler>
    {
        private Mock<ICommandPermissionsGateway> _commandPermissionsGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock; 

        protected override RemoveCommandFromRoleCommandHandler ConstructSystemUnderTest()
        {
            return new RemoveCommandFromRoleCommandHandler
            {
                CommandPermissionsGatewayFactory = () => _commandPermissionsGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandPermissionsGatewayMock = new Mock<ICommandPermissionsGateway>(MockBehavior.Strict);
            _eventPublisherMock = new Mock<IPublishEvent>();
            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallDataGateway_WhenInvoked()
        {
            var command = new RemoveCommandFromRoleCommand
            {
                CommandId = RandomData.RandomInt(10000),
                RoleExternalId = Guid.NewGuid()
            };
            _commandPermissionsGatewayMock.Setup(x => x.RemoveCommandPermission(command.CommandId, command.RoleExternalId));
            _commandPermissionsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _commandPermissionsGatewayMock.Verify(x => x.RemoveCommandPermission(command.CommandId, command.RoleExternalId), Times.Once());
            _eventPublisherMock.Verify(
                e => e.Publish(It.Is<PermissionsUpdatedEvent>(x => x.RoleId == command.RoleExternalId)));
        }
    }
}
