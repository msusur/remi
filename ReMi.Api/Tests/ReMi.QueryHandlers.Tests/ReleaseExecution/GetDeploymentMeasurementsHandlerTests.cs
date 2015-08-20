using System;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Metrics;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.Queries.ReleaseExecution;
using ReMi.QueryHandlers.ReleaseExecution;

namespace ReMi.QueryHandlers.Tests.ReleaseExecution
{
    public class GetDeploymentMeasurementsHandlerTests : TestClassFor<GetDeploymentMeasurementsHandler>
    {
        private Mock<IReleaseDeploymentMeasurementGateway> _releaseJobMeasurementGatewayMock;
        private Mock<IMetricsGateway> _metricsGatewayMock;

        protected override GetDeploymentMeasurementsHandler ConstructSystemUnderTest()
        {
            return new GetDeploymentMeasurementsHandler
            {
                MetricsGatewayFactory = () => _metricsGatewayMock.Object,
                DeploymentMeasurementGatewayFactory = () => _releaseJobMeasurementGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseJobMeasurementGatewayMock = new Mock<IReleaseDeploymentMeasurementGateway>();
            _metricsGatewayMock = new Mock<IMetricsGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetMetrics_WhenInvoked()
        {
            var request = new GetDeploymentMeasurementsRequest
            {
                ReleaseWindowId = Guid.NewGuid(),
            };

            SetupMetrics(request.ReleaseWindowId, new DateTime(2014, 1, 1, 1, 10, 0), new DateTime(2014, 1, 1, 1, 15, 0));

            Sut.Handle(request);

            _metricsGatewayMock.Verify(g => g.GetMetrics(request.ReleaseWindowId, true));
        }

        [Test]
        public void GetMeasurements_ShouldShowOfflineCompletedStage_WhenInvoked()
        {
            var releaseWindowId = Guid.NewGuid();
            SetupMetrics(releaseWindowId, downTime: new DateTime(2014, 1, 1, 1, 10, 0), startTime: new DateTime(2014, 1, 1, 1, 15, 0));

            SetupMeasurements(releaseWindowId, finishTime: new DateTime(2014, 1, 1, 1, 13, 0));

            var request = new GetDeploymentMeasurementsRequest { ReleaseWindowId = releaseWindowId };

            var result = Sut.Handle(request);

            Assert.NotNull(result);
            Assert.NotNull(result.Measurements);
            Assert.AreEqual(JobStage.DuringOffline, result.Measurements.ElementAt(0).JobStage);
        }

        [Test]
        public void GetMeasurements_ShouldShowOnlineBeforeCompletedStage_WhenInvoked()
        {
            var releaseWindowId = Guid.NewGuid();
            SetupMetrics(releaseWindowId, downTime: new DateTime(2014, 1, 1, 1, 10, 0), startTime: new DateTime(2014, 1, 1, 1, 15, 0));

            SetupMeasurements(releaseWindowId, finishTime: new DateTime(2014, 1, 1, 1, 20, 0));

            var request = new GetDeploymentMeasurementsRequest { ReleaseWindowId = releaseWindowId };

            var result = Sut.Handle(request);

            Assert.NotNull(result);
            Assert.NotNull(result.Measurements);
            Assert.AreEqual(JobStage.AfterOffline, result.Measurements.ElementAt(0).JobStage);
        }

        [Test]
        public void GetMeasurements_ShouldShowOnlineAfterCompletedStage_WhenInvoked()
        {
            var releaseWindowId = Guid.NewGuid();
            SetupMetrics(releaseWindowId, downTime: new DateTime(2014, 1, 1, 1, 10, 0), startTime: new DateTime(2014, 1, 1, 1, 15, 0));

            SetupMeasurements(releaseWindowId, finishTime: new DateTime(2014, 1, 1, 1, 0, 0));

            var request = new GetDeploymentMeasurementsRequest { ReleaseWindowId = releaseWindowId };

            var result = Sut.Handle(request);

            Assert.NotNull(result);
            Assert.NotNull(result.Measurements);
            Assert.AreEqual(JobStage.BeforeOffline, result.Measurements.ElementAt(0).JobStage);
        }

        private void SetupMetrics(Guid releaseWindowId, DateTime? downTime = null, DateTime? startTime = null)
        {
            var zeroTime = RandomData.RandomDateTime(DateTimeKind.Utc);

            var items = new[]
            {
                new Metric
                {
                    ExecutedOn = downTime ?? zeroTime.AddMinutes(-RandomData.RandomInt(10, 60)),
                    ExternalId = Guid.NewGuid(),
                    MetricType = MetricType.SiteDown,
                },
                new Metric
                {
                    ExecutedOn = startTime ?? zeroTime.AddMinutes(RandomData.RandomInt(10, 60)),
                    ExternalId = Guid.NewGuid(),
                    MetricType = MetricType.SiteUp,
                }
            };

            _metricsGatewayMock.Setup(o => o.GetMetrics(releaseWindowId, true))
                .Returns(items.ToList());
        }

        private void SetupMeasurements(Guid releaseWindowId, DateTime? finishTime = null)
        {
            var items = new[]
            {
                Builder<JobMeasurement>.CreateNew()
                    .With(o => o.StepId, RandomData.RandomString(10))
                    .With(o => o.FinishTime, finishTime)
                    .Build()
            };

            _releaseJobMeasurementGatewayMock.Setup(o => o.GetDeploymentMeasurements(releaseWindowId)).Returns(items);
        }
    }
}
