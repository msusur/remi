using System;
using Autofac;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Products;
using ReMi.CommandHandlers.Plugins;
using ReMi.Commands.Plugins;
using ReMi.Contracts.Plugins.Services;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.DataAccess.BusinessEntityGateways.Products;

namespace ReMi.CommandHandlers.Tests.Plugins
{
    [TestFixture]
    public class UpdatePluginPackageConfigurationEntityCommandHandlerTests : TestClassFor<UpdatePluginPackageConfigurationEntityCommandHandler>
    {
        private Mock<IContainer> _containerMock;
        private Mock<IPluginGateway> _pluginGatewayMock;
        private Mock<IProductGateway> _packageGatewayMock;

        protected override UpdatePluginPackageConfigurationEntityCommandHandler ConstructSystemUnderTest()
        {
            return new UpdatePluginPackageConfigurationEntityCommandHandler
            {
                Container = _containerMock.Object,
                PluginGatewayFactory = () => _pluginGatewayMock.Object,
                PackageGatewayFactory = () => _packageGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _containerMock = new Mock<IContainer>(MockBehavior.Strict);
            _pluginGatewayMock = new Mock<IPluginGateway>(MockBehavior.Strict);
            _packageGatewayMock = new Mock<IProductGateway>(MockBehavior.Strict);

            _containerMock.SetupResetContainer();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallAssignGlobalPlugin_WhenCalled()
        {
            var command = new UpdatePluginPackageConfigurationEntityCommand
            {
                PluginKey = RandomData.RandomString(10),
                PackageName = RandomData.RandomString(10),
                PropertyName = RandomData.RandomString(10),
                JsonValue = RandomData.RandomString(10)
            };
            var plugin = new BusinessEntities.Plugins.Plugin {PluginId = Guid.NewGuid()};
            var package = new Product {ExternalId = Guid.NewGuid()};
            var pluginPackageConfigurationMock = new Mock<IPluginPackageConfiguration>(MockBehavior.Strict);
            pluginPackageConfigurationMock.Setup(x =>
                x.SetPluginPackageConfigurationEntity(package.ExternalId, command.PropertyName, command.JsonValue));
            _pluginGatewayMock.Setup(x => x.GetPlugin(command.PluginKey)).Returns(plugin);
            _packageGatewayMock.Setup(x => x.GetProduct(command.PackageName)).Returns(package);
            _pluginGatewayMock.Setup(x => x.Dispose());
            _packageGatewayMock.Setup(x => x.Dispose());
            _containerMock.SetupResolveNamed(plugin.PluginId.ToString().ToUpper(), pluginPackageConfigurationMock.Object);

            Sut.Handle(command);

            _pluginGatewayMock.Verify(x => x.GetPlugin(It.IsAny<string>()), Times.Once);
            _packageGatewayMock.Verify(x => x.GetProduct(It.IsAny<string>()), Times.Once);
            _pluginGatewayMock.Verify(x => x.Dispose(), Times.Once);
            _packageGatewayMock.Verify(x => x.Dispose(), Times.Once);
            pluginPackageConfigurationMock.Verify(x =>
                x.SetPluginPackageConfigurationEntity(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
