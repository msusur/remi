using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.Metrics;
using ReMi.CommandHandlers.ContinuousDelivery;
using ReMi.Commands.ContinuousDelivery;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.Events.Metrics;

namespace ReMi.CommandHandlers.Tests.ContinuousDelivery
{
    [TestFixture]
    public class UpdateAutomatedMetricsCommandHandlerTests : TestClassFor<UpdateAutomatedMetricsCommandHandler>
    {
        private Mock<IMetricsGateway> _metricsGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private DateTime _systemTime;

        protected override UpdateAutomatedMetricsCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateAutomatedMetricsCommandHandler
            {
                EventPublisher = _eventPublisherMock.Object,
                MetricsGatewayFactory = () => _metricsGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _eventPublisherMock = new Mock<IPublishEvent>();
            _metricsGatewayMock = new Mock<IMetricsGateway>();
            _systemTime = DateTime.Now;

            SystemTime.Mock(_systemTime);

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(MissingMetricException))]
        public void Handle_ShouldThrowException_WhenMetricNotFound()
        {
            var command = BuildCommand();

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldUpdateMetricAndPublishEvent_WhenMetricFound()
        {
            var command = BuildCommand();
            var metric = new Metric { ExternalId = Guid.NewGuid() };
            _metricsGatewayMock.Setup(x => x.GetMetric(command.ReleaseWindowId, command.MetricType))
                .Returns(metric);

            Sut.Handle(command);

            Assert.AreEqual(_systemTime.ToLocalTime(), metric.ExecutedOn);
            _metricsGatewayMock.Verify(x => x.UpdateMetrics(It.Is<Metric>(
                m => m.ExternalId == metric.ExternalId)), Times.Once);
            _eventPublisherMock.Verify(x => x.Publish(It.Is<MetricsUpdatedEvent>(
                e => e.Metric == metric
                     && e.ReleaseWindowId == command.ReleaseWindowId)), Times.Once);
        }

        private static UpdateAutomatedMetricsCommand BuildCommand()
        {
            return Builder<UpdateAutomatedMetricsCommand>.CreateNew()
                .With(x => x.ReleaseWindowId = Guid.NewGuid())
                .With(x => x.MetricType = RandomData.RandomEnum<MetricType>())
                .Build();
        }
    }
}
