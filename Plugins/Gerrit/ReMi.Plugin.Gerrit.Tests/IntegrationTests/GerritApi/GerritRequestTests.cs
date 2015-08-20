using Moq;
using NUnit.Framework;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Plugin.Gerrit.DataAccess.Gateways;
using ReMi.Plugin.Gerrit.GerritApi;
using ReMi.TestUtils.UnitTests;

namespace ReMi.Plugin.Gerrit.Tests.IntegrationTests.GerritApi
{
    [TestFixture]
    public class GerritRequestTests : TestClassFor<GerritRequest>
    {
        private const string PrivateKey = 
            "-----BEGIN RSA PRIVATE KEY-----\n" +
            "gerrit user private key" +
            "-----END RSA PRIVATE KEY-----";

        private Mock<IGlobalConfigurationGateway> _globalConfigurationGatewayMock;

        protected override GerritRequest ConstructSystemUnderTest()
        {
            return new GerritRequest
            {
                SshClientFactory = () => new SshClient
                {
                    GlobalConfigurationGatewayFactory = () => _globalConfigurationGatewayMock.Object
                }
            };
        }

        protected override void TestInitialize()
        {
            _globalConfigurationGatewayMock = new Mock<IGlobalConfigurationGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test, Explicit]
        public void GetGitLog_ShoudGetDataThroughSSHCommand_WhenCalled()
        {
            var configuration = new PluginConfigurationEntity
            {
                PrivateKey = PrivateKey,
                User = "user",
                Host = "gerrit api address",
                Port = 29418
            };
            _globalConfigurationGatewayMock.Setup(x => x.GetGlobalConfiguration()).Returns(configuration);
            _globalConfigurationGatewayMock.Setup(x => x.Dispose());

            var result = Sut.GetGitLog(new[]
            {
                new ReleaseRepository { Repository = "ff/test", ChangesFrom = "v_3.96", ChangesTo = "release" },
                new ReleaseRepository { Repository = "ff/repo", ChangesFrom = "v_3.89", ChangesTo = "release" }
            });

            Assert.IsNotNull(result);
        }
    }
}
