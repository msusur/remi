using System;
using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.Auth;
using ReMi.QueryHandlers.Auth;

namespace ReMi.QueryHandlers.Tests.Auth
{
    public class PermissionsHandlerTests : TestClassFor<PermissionsHandler>
    {
        private Mock<ICommandPermissionsGateway> _commandPermissionsGatewayMock;
        private Mock<IQueryPermissionsGateway> _queryPermissionsGatewayMock; 

        protected override PermissionsHandler ConstructSystemUnderTest()
        {
            return new PermissionsHandler
            {
                CommandPermissionsGatewayFactory = () => _commandPermissionsGatewayMock.Object,
                QueryPermissionsGatewayFactory = () => _queryPermissionsGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandPermissionsGatewayMock = new Mock<ICommandPermissionsGateway>();
            _queryPermissionsGatewayMock = new Mock<IQueryPermissionsGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewaysToRetrieveApiNames()
        {
            var request = new PermissionsRequest
            {
                RoleId = Guid.NewGuid()
            };

            Sut.Handle(request);

            _commandPermissionsGatewayMock.Verify(c => c.GetAllowedCommands(request.RoleId));
            _queryPermissionsGatewayMock.Verify(s => s.GetAllowedQueries(request.RoleId));
        }
    }
}
