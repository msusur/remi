using System;
using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.Queries.Metrics;
using ReMi.QueryHandlers.Metrics;

namespace ReMi.QueryHandlers.Tests.Metrics
{
    public class MetricsHandlerTests : TestClassFor<GetMetricsHandler>
    {
        private Mock<IMetricsGateway> _metricsGatewayMock;

        protected override GetMetricsHandler ConstructSystemUnderTest()
        {
            return new GetMetricsHandler
            {
                MetricsGatewayFactory = () => _metricsGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _metricsGatewayMock = new Mock<IMetricsGateway>();
            
            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToGetMetrics()
        {
            var request = new GetMetricsRequest {ReleaseWindowId = Guid.NewGuid()};

            Sut.Handle(request);

            _metricsGatewayMock.Verify(m => m.GetMetrics(request.ReleaseWindowId, true));
        }
    }
}
