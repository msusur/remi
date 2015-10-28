using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.Products;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Queries.Metrics;
using ReMi.QueryHandlers.Metrics;
using ReMi.TestUtils.UnitTests;
using System;

namespace ReMi.QueryHandlers.Tests.Metrics
{
    public class MetricsHandlerTests : TestClassFor<GetMetricsHandler>
    {
        private Mock<IMetricsGateway> _metricsGatewayMock;
        private Mock<IProductGateway> _productGatewayFactory;
        private Mock<IDeploymentTool> _deploymentToolService;

        protected override GetMetricsHandler ConstructSystemUnderTest()
        {
            return new GetMetricsHandler
            {
                MetricsGatewayFactory = () => _metricsGatewayMock.Object,
                ProductGatewayFactory = () => _productGatewayFactory.Object,
                DeploymentToolService = _deploymentToolService.Object
            };
        }

        protected override void TestInitialize()
        {
            _metricsGatewayMock = new Mock<IMetricsGateway>(MockBehavior.Strict);
            _productGatewayFactory = new Mock<IProductGateway>(MockBehavior.Strict);
            _deploymentToolService = new Mock<IDeploymentTool>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToGetMetrics()
        {
            var request = new GetMetricsRequest { ReleaseWindowId = Guid.NewGuid() };
            var package = new Product { ExternalId = Guid.NewGuid() };
            var metrics = new Metric[0];

            _metricsGatewayMock.Setup(x => x.Dispose());
            _productGatewayFactory.Setup(x => x.Dispose());

            _metricsGatewayMock.Setup(m => m.GetMetrics(request.ReleaseWindowId, true))
                .Returns(metrics);
            _productGatewayFactory.Setup(x => x.GetProducts(request.ReleaseWindowId))
                .Returns(new[] { package });
            _deploymentToolService.Setup(x => x.AllowGettingDeployTime(package.ExternalId))
                .Returns(true);

            var result = Sut.Handle(request);

            Assert.AreEqual(metrics, result.Metrics);
            Assert.AreEqual(true, result.AutomaticDeployTime);

            _metricsGatewayMock.Verify(m => m.GetMetrics(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
            _metricsGatewayMock.Verify(x => x.Dispose(), Times.Once);
            _productGatewayFactory.Verify(x => x.Dispose(), Times.Once);

        }
    }
}
