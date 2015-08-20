using System;
using Moq;
using NUnit.Framework;
using ReMi.Plugin.Jenkins.DataAccess.Gateways;
using ReMi.Plugin.Jenkins.JenkinsApi;
using ReMi.TestUtils.UnitTests;

namespace ReMi.Plugin.Jenkins.Tests.IntegrationTests
{
    [TestFixture]
    public class JenkinsRequestTests : TestClassFor<JenkinsRequest>
    {
        private Mock<IGlobalConfigurationGateway> _globalConfigurationGatewayMock;

        protected override JenkinsRequest ConstructSystemUnderTest()
        {
            return new JenkinsRequest
            {
                GlobalConfigurationGatewayFactory = () => _globalConfigurationGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            var config = new PluginConfigurationEntity
            {
                // TODO Fillout before tests
                JenkinsPassword = "",
                JenkinsUser = ""
            };
            _globalConfigurationGatewayMock = new Mock<IGlobalConfigurationGateway>(MockBehavior.Strict);
            _globalConfigurationGatewayMock.Setup(x => x.GetGlobalConfiguration())
                .Returns(config);
            _globalConfigurationGatewayMock.Setup(x => x.Dispose());

            if (string.IsNullOrEmpty(config.JenkinsPassword) || string.IsNullOrEmpty(config.JenkinsUser))
                throw new ApplicationException("Fill out username and password");
            base.TestInitialize();
        }

        [Test, Explicit]
        public void GetJobInfo_ShouldGetJobInfo_WhenCalled()
        {
            Sut.ChangeBaseUrl("https://build.com");
            var jobInfo = Sut.GetJobInfo("deploy-multi-master-pl_rc");

            Assert.IsNotNull(jobInfo);
        }

        [Test, Explicit]
        public void GetBuildInfo_ShouldGetBuildInfo_WhenCalled()
        {
            Sut.ChangeBaseUrl("https://build.com");
            var buildInfo = Sut.GetBuildInfo("deploy-multi-master-pl_rc", 3);

            Assert.IsNotNull(buildInfo);
        }

        [Test, Explicit]
        public void GetJobMetrics_ShouldGetJobMetrics_WhenCalled()
        {
            Sut.ChangeBaseUrl("http://release.build.com/");
            var metrics = Sut.GetJobMetrics("ReMi_Live-deploy", 9, TimeZone.Bst);

            Assert.IsNotNull(metrics);
        }
    }
}
