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
    public class AddQueryToRoleQueryHandlerTests : TestClassFor<AddQueryToRoleCommandHandler>
    {
        private Mock<IQueryPermissionsGateway> _queryPermissionsGatewayMock;
        public Mock<IPublishEvent> _eventPublisherMock;

        protected override AddQueryToRoleCommandHandler ConstructSystemUnderTest()
        {
            return new AddQueryToRoleCommandHandler
            {
                QueryPermissionsGatewayFactory = () => _queryPermissionsGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _queryPermissionsGatewayMock = new Mock<IQueryPermissionsGateway>(MockBehavior.Strict);
            _eventPublisherMock = new Mock<IPublishEvent>();
            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallDataGateway_WhenInvoked()
        {
            var query = new AddQueryToRoleCommand
            {
                QueryId = RandomData.RandomInt(10000),
                RoleExternalId = Guid.NewGuid()
            };
            _queryPermissionsGatewayMock.Setup(x => x.AddQueryPermission(query.QueryId, query.RoleExternalId));
            _queryPermissionsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(query);

            _queryPermissionsGatewayMock.Verify(x => x.AddQueryPermission(query.QueryId, query.RoleExternalId), Times.Once());

            _eventPublisherMock.Verify(
                e => e.Publish(It.Is<PermissionsUpdatedEvent>(x => x.RoleId == query.RoleExternalId)));
        }
    }
}
