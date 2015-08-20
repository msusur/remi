using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic.Metrics;
using ReMi.Common.Constants.Metrics;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Enums;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.Exceptions.Configuration;
using ReMi.Queries.Metrics;
using ReMi.QueryHandlers.Metrics;

namespace ReMi.QueryHandlers.Tests.Metrics
{
    public class MeasurementHandlerTests : TestClassFor<GetMeasurementsHandler>
    {
        private Mock<IMetricsGateway> _metricGatewayMock;
        private Mock<IProductGateway> _productGatewayMock;
        private Mock<IMeasurementsCalculator> _measurementsCalculatorMock;

        protected override GetMeasurementsHandler ConstructSystemUnderTest()
        {
            return new GetMeasurementsHandler
            {
                MetricsGatewayFactory = () => _metricGatewayMock.Object,
                MeasurementsCalculator = _measurementsCalculatorMock.Object,
                ProductGatewayFactory = () => _productGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _metricGatewayMock = new Mock<IMetricsGateway>();
            _productGatewayMock = new Mock<IProductGateway>();
            _measurementsCalculatorMock = new Mock<IMeasurementsCalculator>();
            _measurementsCalculatorMock.Setup(
                o =>
                    o.Calculate(It.IsAny<ReleaseWindow>(), It.IsAny<IEnumerable<Metric>>(),
                        It.IsAny<MeasurementType[]>()))
                .Returns((Measurement)null);

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(ProductNotFoundException))]
        public void Handle_ShouldThrowProductNotFoundException_WhenRequestedProductNotExists()
        {
            var request = new GetMeasurementsRequest
            {
                Product = RandomData.RandomString(10)
            };

            Sut.Handle(request);
        }

        [Test]
        public void Handle_ShouldCallMetricGateway_WhenInvoked()
        {
            var product = SetupProduct("prod");

            SetupReleaseWindow("prod");

            var request = new GetMeasurementsRequest
            {
                Product = product.Description
            };

            Sut.Handle(request);

            _metricGatewayMock.Verify(m => m.GetMetrics(request.Product));
        }

        [Test]
        public void Handle_ShouldReturnAutomatedRelease_WhenProductReleaseTrackIsAutomated()
        {
            var product = SetupProduct("prod", ReleaseTrack.Automated);

            var releaseWindow = SetupReleaseWindow("prod", ReleaseType.Automated);

            _measurementsCalculatorMock
                .Setup(o => o.Calculate(It.IsAny<ReleaseWindow>(), It.IsAny<IEnumerable<Metric>>(), It.IsAny<MeasurementType[]>()))
                .Returns(new Measurement
                {
                    Metrics = new[]
                    {
                        new MeasurementTime{ Name = RandomData.RandomString(10), Value = RandomData.RandomInt(100)}
                    }
                });

            var request = new GetMeasurementsRequest
            {
                Product = product.Description
            };

            var result = Sut.Handle(request);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Measurements.Count());

            _measurementsCalculatorMock
                .Verify(o =>
                    o.Calculate(It.Is<ReleaseWindow>(x =>
                        x.ExternalId == releaseWindow.ExternalId), It.IsAny<IEnumerable<Metric>>(), MeasurementType.OverallTime, MeasurementType.DeployTime));
        }

        [Test]
        public void Handle_ShouldUseSpecificMetrics_WhenRequestedReleaseScheduled()
        {
            var product = SetupProduct("prod");

            var releaseWindow = SetupReleaseWindow("prod");

            _measurementsCalculatorMock
                .Setup(
                    o =>
                        o.Calculate(It.IsAny<ReleaseWindow>(), It.IsAny<IEnumerable<Metric>>(),
                            It.IsAny<MeasurementType[]>()))
                .Returns(new Measurement
                {
                    Metrics = new[]
                    {
                        new MeasurementTime
                        {
                            Name = EnumDescriptionHelper.GetDescription(MeasurementType.OverallTime),
                            Value = RandomData.RandomInt(100)
                        },
                        new MeasurementTime
                        {
                            Name = EnumDescriptionHelper.GetDescription(MeasurementType.DeployTime),
                            Value = RandomData.RandomInt(100)
                        }
                    }
                });

            var request = new GetMeasurementsRequest { Product = product.Description };

            var result = Sut.Handle(request);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Measurements.Count());

            _measurementsCalculatorMock
                .Verify(o =>
                    o.Calculate(It.Is<ReleaseWindow>(x =>
                        x.ExternalId == releaseWindow.ExternalId), It.IsAny<IEnumerable<Metric>>(), MeasurementType.OverallTime, MeasurementType.DeployTime, MeasurementType.DownTime));
        }

        private Product SetupProduct(string productName, ReleaseTrack trackType = ReleaseTrack.Manual)
        {
            var product = Builder<Product>.CreateNew()
                .With(o => o.Description, productName)
                .With(o => o.ReleaseTrack, trackType)
                .Build();

            _productGatewayMock.Setup(o => o.GetProduct(productName))
                .Returns(product);

            return product;
        }

        private ReleaseWindow SetupReleaseWindow(string product, ReleaseType releaseType = ReleaseType.Scheduled)
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Products, new[] { product })
                .With(o => o.ReleaseType, releaseType)
                .Build();

            _metricGatewayMock.Setup(o => o.GetMetrics(It.IsAny<string>()))
                .Returns(new Dictionary<ReleaseWindow, IEnumerable<Metric>>
                {
                    {releaseWindow, Enumerable.Empty<Metric>()}
                });

            return releaseWindow;
        }
    }
}
