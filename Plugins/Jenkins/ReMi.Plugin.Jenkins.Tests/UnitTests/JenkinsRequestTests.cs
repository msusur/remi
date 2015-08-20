using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.Plugin.Common.Serialization;
using ReMi.Plugin.Jenkins.DataAccess.Gateways;
using ReMi.Plugin.Jenkins.JenkinsApi;
using ReMi.Plugin.Jenkins.JenkinsApi.Model;
using ReMi.TestUtils.UnitTests;
using RestSharp;

namespace ReMi.Plugin.Jenkins.Tests.UnitTests
{
    public class JenkinsRequestMock : JenkinsRequest
    {
        private readonly RestClient _client;
        protected override RestClient Client { get { return _client; } }

        public JenkinsRequestMock(RestClient client)
        {
            _client = client;
        }
    }

    [TestFixture]
    public class JenkinsRequestTests : TestClassFor<JenkinsRequestMock>
    {
        private Mock<IGlobalConfigurationGateway> _globalConfigurationGatewayMock;
        private Mock<RestClient> _restClientMock;

        protected override JenkinsRequestMock ConstructSystemUnderTest()
        {
            return new JenkinsRequestMock(_restClientMock.Object)
            {
                GlobalConfigurationGatewayFactory = () => _globalConfigurationGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            var config = new PluginConfigurationEntity();
            _globalConfigurationGatewayMock = new Mock<IGlobalConfigurationGateway>(MockBehavior.Strict);
            _restClientMock = new Mock<RestClient>(MockBehavior.Strict);
            _globalConfigurationGatewayMock.Setup(x => x.GetGlobalConfiguration())
                .Returns(config);
            _globalConfigurationGatewayMock.Setup(x => x.Dispose());

            base.TestInitialize();
        }

        private IRestResponse<T> GetResponse<T>(string jsonData)
        {
            var mock = new Mock<IRestResponse<T>>();
            mock.SetupGet(x => x.Data).Returns(JsonSerializationImpl.Deserialize<T>(jsonData));
            mock.SetupGet(x => x.Content).Returns(jsonData);
            return mock.Object;
        }

        [Test]
        public void GetJobInfo_ShouldGetJobInfo_WhenCalled()
        {
            _restClientMock.Setup(x => x.Execute<JobInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/deploy-multi-master-pl_rc/api/json")))
                .Returns(GetResponse<JobInfo>(File.ReadAllText(@"TestData\jobInfo_deploy-multi-master-pl_rc.json")));

            var jobInfo = Sut.GetJobInfo("deploy-multi-master-pl_rc");

            Assert.IsNotNull(jobInfo);

        }

        [Test]
        public void GetBuildInfo_ShouldGetBuildInfo_WhenCalled()
        {
            _restClientMock.Setup(x => x.Execute<BuildInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/deploy-multi-master-pl_rc/3/api/json")))
                .Returns(GetResponse<BuildInfo>(File.ReadAllText(@"TestData\buildInfo_3_deploy-multi-master-pl_rc.json")));

            var buildInfo = Sut.GetBuildInfo("deploy-multi-master-pl_rc", 3);
            
            Assert.IsNotNull(buildInfo);
            Assert.AreEqual(DateTime.ParseExact("May 12, 2015 4:45:32 PM", "MMM dd, yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                buildInfo.GetStartTime(TimeZone.Utc));
        }

        [Test]
        public void GetJobMetrics_ShouldGetJobMetrics_WhenCalled()
        {
            _restClientMock.Setup(x => x.Execute<JobInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/deploy-multi-master-pl_rc/api/json")))
                .Returns(GetResponse<JobInfo>(File.ReadAllText(@"TestData\jobInfo_deploy-multi-master-pl_rc.json")));
            _restClientMock.Setup(x => x.Execute<BuildInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/deploy-multi-master-pl_rc/3/api/json")))
                .Returns(GetResponse<BuildInfo>(File.ReadAllText(@"TestData\buildInfo_3_deploy-multi-master-pl_rc.json")));
            _restClientMock.Setup(x => x.Execute<BuildInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/deploy-multi-master-pl_rc/4/api/json")))
                .Returns(GetResponse<BuildInfo>(File.ReadAllText(@"TestData\buildInfo_4_deploy-multi-master-pl_rc.json")));

            _restClientMock.Setup(x => x.Execute<BuildInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/tc.kontomierz-deploy/10/api/json")))
                .Returns(GetResponse<BuildInfo>(File.ReadAllText(@"TestData\buildInfo_10_tc.kontomierz-deploy.json")));
            _restClientMock.Setup(x => x.Execute<BuildInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/ops-deploy/16/api/json")))
                .Returns(GetResponse<BuildInfo>(File.ReadAllText(@"TestData\buildInfo_16_ops-deploy.json")));
            _restClientMock.Setup(x => x.Execute<BuildInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/marketing-deploy/5/api/json")))
                .Returns(GetResponse<BuildInfo>(File.ReadAllText(@"TestData\buildInfo_5_marketing-deploy.json")));
            _restClientMock.Setup(x => x.Execute<BuildInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/merge.callback-deploy/6/api/json")))
                .Returns(GetResponse<BuildInfo>(File.ReadAllText(@"TestData\buildInfo_6_merge.callback-deploy.json")));
            _restClientMock.Setup(x => x.Execute<BuildInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/merge.cswebapi-deploy/6/api/json")))
                .Returns(GetResponse<BuildInfo>(File.ReadAllText(@"TestData\buildInfo_6_merge.cswebapi-deploy.json")));
            _restClientMock.Setup(x => x.Execute<BuildInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/segmentengine-deploy/6/api/json")))
                .Returns(GetResponse<BuildInfo>(File.ReadAllText(@"TestData\buildInfo_6_segmentengine-deploy.json")));
            _restClientMock.Setup(x => x.Execute<BuildInfo>(It.Is<IRestRequest>(r =>
                    r.Resource == "job/tc.timezone-deploy/8/api/json")))
                .Returns(GetResponse<BuildInfo>(File.ReadAllText(@"TestData\buildInfo_8_tc.timezone-deploy.json")));

            var metrics = Sut.GetJobMetrics("deploy-multi-master-pl_rc", 2, TimeZone.Bst);

            Assert.IsNotNull(metrics);
            Assert.AreEqual(4, metrics.BuildNumber);
            Assert.AreEqual("2015-05-12_18-08-40", metrics.JobId);
            Assert.AreEqual(2, metrics.NumberOfTries);
            Assert.AreEqual("deploy-multi-master-pl_rc", metrics.Name);
            Assert.AreEqual("https://build.com/job/deploy-multi-master-pl_rc/4/", metrics.Url);
            Assert.AreEqual(DateTime.ParseExact("May 12, 2015 4:45:32 PM", "MMM dd, yyyy h:mm:ss tt", CultureInfo.InvariantCulture).ToLongTimeString(),
                metrics.StartTime.ToLongTimeString());
            Assert.AreEqual(DateTime.ParseExact("May 12, 2015 5:14:43 PM", "MMM dd, yyyy h:mm:ss tt", CultureInfo.InvariantCulture).ToLongTimeString(),
                metrics.EndTime.ToLongTimeString());

            Assert.AreEqual(4, metrics.ChildMetrics.Count(), "Phases count");

            Assert.AreEqual(1, metrics.ChildMetrics.ElementAt(0).ChildMetrics.Count(), "Phases 1 jobs count");
            Assert.AreEqual(1, metrics.ChildMetrics.ElementAt(0).NumberOfTries);
            Assert.AreEqual("core", metrics.ChildMetrics.ElementAt(0).Name);
            Assert.AreEqual(4, metrics.ChildMetrics.ElementAt(0).BuildNumber);
            Assert.AreEqual(metrics.ChildMetrics.ElementAt(0).ChildMetrics.First().StartTime, metrics.ChildMetrics.ElementAt(0).StartTime);
            Assert.AreEqual(metrics.ChildMetrics.ElementAt(0).ChildMetrics.First().EndTime, metrics.ChildMetrics.ElementAt(0).EndTime);

            Assert.AreEqual(2, metrics.ChildMetrics.ElementAt(1).ChildMetrics.Count(), "Phases 2 jobs count");

            Assert.AreEqual(2, metrics.ChildMetrics.ElementAt(2).ChildMetrics.Count(), "Phases 3 jobs count");
            Assert.AreEqual(2, metrics.ChildMetrics.ElementAt(2).NumberOfTries);

            Assert.AreEqual(2, metrics.ChildMetrics.ElementAt(3).ChildMetrics.Count(), "Phases 4 jobs count");
            //Assert.Contains();
        }
    }
}
