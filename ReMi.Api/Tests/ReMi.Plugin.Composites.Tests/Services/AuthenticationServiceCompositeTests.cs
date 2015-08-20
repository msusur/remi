using System;
using Autofac;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Plugins;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.Authentication;
using ReMi.Contracts.Plugins.Services.Authentication;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.Plugin.Composites.Services;

namespace ReMi.Plugin.Composites.Tests.Services
{
    public class AuthenticationServiceCompositeTests : TestClassFor<AuthenticationServiceComposite>
    {
        private Mock<IPluginGateway> _pluginGatewayMock;
        private Mock<IContainer> _containerMock;

        protected override AuthenticationServiceComposite ConstructSystemUnderTest()
        {
            return new AuthenticationServiceComposite
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
        public void GetAccount_ShouldReturnNull_WhenPluginTypeNotFound()
        {
            var packageConfiguration = new GlobalPluginConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginType = PluginType.Email
            };
            var userName = RandomData.RandomString(10);
            var password = RandomData.RandomString(10);

            _pluginGatewayMock.Setup(x => x.GetGlobalPluginConfiguration())
                .Returns(new[] { packageConfiguration });
            _pluginGatewayMock.Setup(x => x.Dispose());

            var result = Sut.GetAccount(userName, password);

            Assert.IsNull(result);
            _pluginGatewayMock.Verify(x => x.GetGlobalPluginConfiguration(), Times.Once);
            _pluginGatewayMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void GetAccount_ShouldReturnNull_WhenServiceNotAssignedToPlugin()
        {
            var packageConfiguration = new GlobalPluginConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginType = PluginType.Authentication
            };
            var userName = RandomData.RandomString(10);
            var password = RandomData.RandomString(10);

            _pluginGatewayMock.Setup(x => x.GetGlobalPluginConfiguration())
                .Returns(new[] { packageConfiguration });
            _pluginGatewayMock.Setup(x => x.Dispose());

            var result = Sut.GetAccount(userName, password);

            Assert.IsNull(result);
            _pluginGatewayMock.Verify(x => x.GetGlobalPluginConfiguration(), Times.Once);
            _pluginGatewayMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void GetAccount_ShouldReturnSeviceResult_WhenServiceFound()
        {
            var packageConfiguration = new GlobalPluginConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginType = PluginType.Authentication,
                PluginId = Guid.NewGuid()
            };
            var userName = RandomData.RandomString(10);
            var password = RandomData.RandomString(10);
            var account = new Account();
            var service = new Mock<IAuthenticationService>(MockBehavior.Strict);

            _pluginGatewayMock.Setup(x => x.GetGlobalPluginConfiguration())
                .Returns(new[] { packageConfiguration });
            _pluginGatewayMock.Setup(x => x.Dispose());
            _containerMock.SetupResolveNamed(packageConfiguration.PluginId.ToString().ToUpper(), service.Object);
            service.Setup(x => x.GetAccount(userName, password))
                .Returns(account);

            var result = Sut.GetAccount(userName, password);

            Assert.AreEqual(account, result);
            _pluginGatewayMock.Verify(x => x.GetGlobalPluginConfiguration(), Times.Once);
            _pluginGatewayMock.Verify(x => x.Dispose(), Times.Once);
            service.Verify(x => x.GetAccount(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
