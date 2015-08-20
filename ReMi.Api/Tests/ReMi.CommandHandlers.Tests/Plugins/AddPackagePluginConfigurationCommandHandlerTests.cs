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
    public class AddPackagePluginConfigurationCommandHandlerTests : TestClassFor<AddPackagePluginConfigurationCommandHandler>
    {
        private Mock<IPluginGateway> _pluginGatewayMock;

        protected override AddPackagePluginConfigurationCommandHandler ConstructSystemUnderTest()
        {
            return new AddPackagePluginConfigurationCommandHandler
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
        public void Handle_ShouldCallAddPluginPackageConfiguration_WhenCalled()
        {
            var command = new AddPackagePluginConfigurationCommand
            {
                PackageId = Guid.NewGuid()
            };

            _pluginGatewayMock.Setup(x => x.AddPluginPackageConfiguration(command.PackageId));
            _pluginGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _pluginGatewayMock.Verify(x => x.AddPluginPackageConfiguration(It.IsAny<Guid>()), Times.Once);
        }
    }
}
