using System;
using Autofac;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.Plugins;
using ReMi.Commands.Plugins;
using ReMi.Contracts.Plugins.Services;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;

namespace ReMi.CommandHandlers.Tests.Plugins
{
    [TestFixture]
    public class UpdatePluginGlobalConfigurationEntityCommandHandlerTests : TestClassFor<UpdatePluginGlobalConfigurationEntityCommandHandler>
    {
        private Mock<IContainer> _containerMock;
        private Mock<IPluginGateway> _pluginGatewayMock;

        protected override UpdatePluginGlobalConfigurationEntityCommandHandler ConstructSystemUnderTest()
        {
            return new UpdatePluginGlobalConfigurationEntityCommandHandler
            {
                Container = _containerMock.Object,
                PluginGatewayFactory = () => _pluginGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _containerMock = new Mock<IContainer>(MockBehavior.Strict);
            _pluginGatewayMock = new Mock<IPluginGateway>(MockBehavior.Strict);

            _containerMock.SetupResetContainer();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallAssignGlobalPlugin_WhenCalled()
        {
            var command = new UpdatePluginGlobalConfigurationEntityCommand
            {
                PluginKey = RandomData.RandomString(10),
                PropertyName = RandomData.RandomString(10),
                JsonValue = RandomData.RandomString(10)
            };
            var plugin = new BusinessEntities.Plugins.Plugin {PluginId = Guid.NewGuid()};
            var pluginConfigurationMock = new Mock<IPluginConfiguration>(MockBehavior.Strict);
            pluginConfigurationMock.Setup(x => x.SetPluginConfiguration(command.PropertyName, command.JsonValue));
            _pluginGatewayMock.Setup(x => x.GetPlugin(command.PluginKey)).Returns(plugin);
            _pluginGatewayMock.Setup(x => x.Dispose());
            _containerMock.SetupResolveNamed(plugin.PluginId.ToString().ToUpper(), pluginConfigurationMock.Object);

            Sut.Handle(command);

            _pluginGatewayMock.Verify(x => x.GetPlugin(It.IsAny<string>()), Times.Once);
            _pluginGatewayMock.Verify(x => x.Dispose(), Times.Once);
            pluginConfigurationMock.Verify(x => x.SetPluginConfiguration(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
