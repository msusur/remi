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
    public class AssignGlobalPluginCommandHandlerTests : TestClassFor<AssignGlobalPluginCommandHandler>
    {
        private Mock<IPluginGateway> _pluginGatewayMock;

        protected override AssignGlobalPluginCommandHandler ConstructSystemUnderTest()
        {
            return new AssignGlobalPluginCommandHandler
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
        public void Handle_ShouldCallAssignGlobalPlugin_WhenCalled()
        {
            var command = new AssignGlobalPluginCommand
            {
                ConfigurationId = Guid.NewGuid(),
                PluginId = Guid.NewGuid()
            };

            _pluginGatewayMock.Setup(x => x.AssignGlobalPlugin(command.ConfigurationId, command.PluginId));
            _pluginGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _pluginGatewayMock.Verify(x => x.AssignGlobalPlugin(It.IsAny<Guid>(), It.IsAny<Guid?>()), Times.Once);
        }
    }
}
