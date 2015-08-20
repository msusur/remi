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
    public class GetGlobalPluginConfigurationHandlerTests : TestClassFor<GetGlobalPluginConfigurationHandler>
    {
        private Mock<IPluginGateway> _pluginGateway;

        protected override GetGlobalPluginConfigurationHandler ConstructSystemUnderTest()
        {
            return new GetGlobalPluginConfigurationHandler
            {
                PluginGatewayFactory = () => _pluginGateway.Object
            };
        }

        protected override void TestInitialize()
        {
            _pluginGateway = new Mock<IPluginGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetGlobalPluginConfigurationAndGlobalPlugins_WhenCalled()
        {
            var globalPluginConfiguration = new[] {
                new GlobalPluginConfiguration { ExternalId = Guid.NewGuid(), PluginType = PluginType.Authentication },
                new GlobalPluginConfiguration { ExternalId = Guid.NewGuid(), PluginType = PluginType.Email }
            };
            var plugins = new[]
            {
                new BusinessEntities.Plugins.Plugin {PluginTypes = new[] { PluginType.QaStats, PluginType.Authentication, PluginType.DeploymentTool }},
                new BusinessEntities.Plugins.Plugin {PluginTypes = new[] { PluginType.Email }},
                new BusinessEntities.Plugins.Plugin {PluginTypes = new[] { PluginType.HelpDesk }}
            };
            _pluginGateway.Setup(x => x.GetGlobalPluginConfiguration())
                .Returns(globalPluginConfiguration);
            _pluginGateway.Setup(x => x.GetPlugins())
                .Returns(plugins);
            _pluginGateway.Setup(x => x.Dispose());

            var response = Sut.Handle(new GetGlobalPluginConfigurationRequest());

            Assert.AreEqual(globalPluginConfiguration, response.GlobalPluginConfiguration);
            Assert.AreEqual(2, response.GlobalPlugins.Count());

            _pluginGateway.Verify(x => x.GetGlobalPluginConfiguration(), Times.Once);
            _pluginGateway.Verify(x => x.GetPlugins(), Times.Once);
        }
    }
}
