using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.Queries.ReleaseExecution;
using ReMi.QueryHandlers.Metrics;

namespace ReMi.QueryHandlers.Tests.Metrics
{
    public class GetDeploymentJobMeasurementsByProductHandlerTests : TestClassFor<GetDeploymentJobMeasurementsByProductHandler>
    {
        private Mock<IMappingEngine> _mappingEngineMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IReleaseDeploymentMeasurementGateway> _releaseJobMeasurementGatewayMock;

        protected override GetDeploymentJobMeasurementsByProductHandler ConstructSystemUnderTest()
        {
            return new GetDeploymentJobMeasurementsByProductHandler
            {
                MappingEngine = _mappingEngineMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                ReleaseDeploymentJobMeasurementGatewayFactory = () => _releaseJobMeasurementGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _mappingEngineMock = new Mock<IMappingEngine>();
            _mappingEngineMock.Setup(o => o.Map<JobMeasurement, MeasurementTime>(It.IsAny<JobMeasurement>()))
                .Returns<JobMeasurement>(m =>
                {
                    return new MeasurementTime { Name = m.StepName, Value = Convert.ToInt32(m.Duration) };
                });

            _releaseJobMeasurementGatewayMock = new Mock<IReleaseDeploymentMeasurementGateway>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldReturnEmptyResponse_WhenNoDataPresent()
        {
            var req = new GetDeploymentJobMeasurementsByProductRequest { Product = "prod" };

            var result = Sut.Handle(req);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Measurements.Count());
        }

        [Test]
        public void Handle_ShouldReturnMeasurements_WhenInvoked()
        {
            var req = new GetDeploymentJobMeasurementsByProductRequest { Product = "prod" };

            SetupReleaseWindows("prod");
            SetupMeasurements(10);

            var result = Sut.Handle(req);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Measurements.Count());
            Assert.AreEqual(10, result.Measurements.ElementAt(0).Metrics.Count());
        }

        private IEnumerable<ReleaseWindow> SetupReleaseWindows(string product, Guid[] externalIds = null)
        {
            var ids = externalIds ?? new[] { Guid.NewGuid() };

            var releasseWindows = new List<ReleaseWindow>();
            foreach (var id in ids)
            {
                var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                    .With(o => o.ExternalId, id)
                    .Build();

                releasseWindows.Add(releaseWindow);
            }

            _releaseWindowGatewayMock.Setup(o => o.GetAllByProduct(product)).Returns(releasseWindows);

            return releasseWindows;
        }

        private void SetupMeasurements(int metricCount)
        {
            var stepMeasurements = Builder<JobMeasurement>.CreateListOfSize(metricCount)
                .All()
                .Do(item =>
                {
                    item.StartTime = RandomData.RandomDateTime();
                    item.FinishTime = ((DateTime)item.StartTime).AddMinutes(RandomData.RandomInt(200));
                })
                .Build();

            _releaseJobMeasurementGatewayMock.Setup(o => o.GetDeploymentMeasurements(It.IsAny<Guid>()))
                .Returns(stepMeasurements);
        }
    }
}
