using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.Configuration;
using ReMi.QueryHandlers.Configuration;

namespace ReMi.QueryHandlers.Tests.Configuration
{
    public class GetCommandsWithRolesHandlerTests : TestClassFor<GetCommandsWithRolesHandler>
    {
        private Mock<ICommandPermissionsGateway> _commandPermissionsGatewayMock;

        protected override GetCommandsWithRolesHandler ConstructSystemUnderTest()
        {
            return new GetCommandsWithRolesHandler
            {
                CommandPermissionsGatewayFactory = () => _commandPermissionsGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandPermissionsGatewayMock = new Mock<ICommandPermissionsGateway>(MockBehavior.Strict);
            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallDataGateway_WhenInvoked()
        {
            var request = new GetCommandsWithRolesRequest();
            var expected = new List<Command>();

            _commandPermissionsGatewayMock.Setup(x => x.GetCommands(false))
                .Returns(expected);
            _commandPermissionsGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            Assert.AreEqual(expected, result.Commands);
            _commandPermissionsGatewayMock.Verify(x => x.GetCommands(false), Times.Once());
        }
    }
}
