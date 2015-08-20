using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Plugins;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.Queries.Plugins;
using ReMi.QueryHandlers.Plugins;
using System;
using System.Linq;

namespace ReMi.QueryHandlers.Tests.Plugins
{
    [TestFixture]
    public class GetPackagePluginConfigurationHandlerTests : TestClassFor<GetPackagePluginConfigurationHandler>
    {
        private Mock<IPluginGateway> _pluginGatewayMock;

        protected override GetPackagePluginConfigurationHandler ConstructSystemUnderTest()
        {
            return new GetPackagePluginConfigurationHandler
            {
                PluginGatewayFactory = () => _pluginGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _pluginGatewayMock = new Mock<IPluginGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetPackagePluginConfigurationAndPackagePlugins_WhenCalled()
        {
            var packageId = Guid.NewGuid();
            var packageGlobalPluginConfiguration = new[] {
                new PackagePluginConfiguration { ExternalId = Guid.NewGuid(), PackageId = packageId, PluginType = PluginType.QaStats },
                new PackagePluginConfiguration { ExternalId = Guid.NewGuid(), PackageId = packageId, PluginType = PluginType.HelpDesk },
                new PackagePluginConfiguration { ExternalId = Guid.NewGuid(), PackageId = Guid.NewGuid(), PluginType = PluginType.ReleaseContent }
            };
            var plugins = new[]
            {
                new BusinessEntities.Plugins.Plugin {PluginTypes = new[] { PluginType.QaStats, PluginType.Authentication, PluginType.DeploymentTool }},
                new BusinessEntities.Plugins.Plugin {PluginTypes = new[] { PluginType.Email }},
                new BusinessEntities.Plugins.Plugin {PluginTypes = new[] { PluginType.HelpDesk }}
            };
            _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration())
                .Returns(packageGlobalPluginConfiguration);
            _pluginGatewayMock.Setup(x => x.GetPlugins())
                .Returns(plugins);
            _pluginGatewayMock.Setup(x => x.Dispose());

            var response = Sut.Handle(new GetPackagePluginConfigurationRequest());

            Assert.AreEqual(2, response.PackagePluginConfiguration.Count);
            Assert.AreEqual(2, response.PackagePluginConfiguration[packageId].Count);
            Assert.AreEqual(2, response.PackagePlugins.Count());

            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(), Times.Once);
            _pluginGatewayMock.Verify(x => x.GetPlugins(), Times.Once);
        }
    }
}
