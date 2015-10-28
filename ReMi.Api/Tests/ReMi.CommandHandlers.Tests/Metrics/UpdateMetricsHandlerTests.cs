using System;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Metrics;
using ReMi.CommandHandlers.Metrics;
using ReMi.Commands.Metrics;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.Events.Metrics;

namespace ReMi.CommandHandlers.Tests.Metrics
{
    public class UpdateMetricsHandlerTests : TestClassFor<UpdateMetricsCommandHandler>
    {
        private Mock<IMetricsGateway> _metricsGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Metric _metric;

        protected override UpdateMetricsCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateMetricsCommandHandler
            {
                MetricsGatewayFactory = () => _metricsGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _eventPublisherMock = new Mock<IPublishEvent>();
            _metricsGatewayMock = new Mock<IMetricsGateway>();
            _metric = new Metric();
            

            base.TestInitialize();
        }

        [Test]
        public void Handle_ActionPerformed()
        {
            var updateMetricsCommand = new UpdateMetricsCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                Metric = new Metric()
            };

            Sut.Handle(updateMetricsCommand);

            _metricsGatewayMock.Verify(m => m.UpdateMetrics(It.Is<Metric>(c => c.ExecutedOn.HasValue)));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<MetricsUpdatedEvent>(
                            m => m.Metric.ExecutedOn.HasValue && m.ReleaseWindowId == updateMetricsCommand.ReleaseWindowId)));
        }
    }
}
