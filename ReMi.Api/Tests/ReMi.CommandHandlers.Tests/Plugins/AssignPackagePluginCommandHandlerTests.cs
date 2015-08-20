using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.Plugins;
using ReMi.Commands.Plugins;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using System;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandHandlers.Tests.Plugins
{
    [TestFixture]
    public class AssignPackagePluginCommandHandlerTests : TestClassFor<AssignPackagePluginCommandHandler>
    {
        private Mock<IPluginGateway> _pluginGatewayMock;

        protected override AssignPackagePluginCommandHandler ConstructSystemUnderTest()
        {
            return new AssignPackagePluginCommandHandler
            {
                PluginGatewayFactory = () => _pluginGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            base.TestInitialize();
            _pluginGatewayMock = new Mock<IPluginGateway>(MockBehavior.Strict);
        }

        [Test]
        public void Handle_ShouldCallAssignPackagePlugin_WhenCalled()
        {
            var command = new AssignPackagePluginCommand
            {
                ConfigurationId = Guid.NewGuid(),
                PluginId = Guid.NewGuid()
            };

            _pluginGatewayMock.Setup(x => x.AssignPackagePlugin(command.ConfigurationId, command.PluginId));
            _pluginGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _pluginGatewayMock.Verify(x => x.AssignPackagePlugin(It.IsAny<Guid>(), It.IsAny<Guid?>()), Times.Once);
        }
    }
}
