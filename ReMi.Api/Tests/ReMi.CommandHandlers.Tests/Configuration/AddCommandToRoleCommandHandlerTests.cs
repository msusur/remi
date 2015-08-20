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
    [TestFixture]
    public class AddCommandToRoleCommandHandlerTests : TestClassFor<AddCommandToRoleCommandHandler>
    {
        private Mock<ICommandPermissionsGateway> _commandPermissionsGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override AddCommandToRoleCommandHandler ConstructSystemUnderTest()
        {
            return new AddCommandToRoleCommandHandler
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
            var command = new AddCommandToRoleCommand
            {
                CommandId = RandomData.RandomInt(10000),
                RoleExternalId = Guid.NewGuid()
            };
            _commandPermissionsGatewayMock.Setup(x => x.AddCommandPermission(command.CommandId, command.RoleExternalId));
            _commandPermissionsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _commandPermissionsGatewayMock.Verify(x => x.AddCommandPermission(command.CommandId, command.RoleExternalId), Times.Once());
            _eventPublisherMock.Verify(
                e => e.Publish(It.Is<PermissionsUpdatedEvent>(x => x.RoleId == command.RoleExternalId)));
        }
    }
}
