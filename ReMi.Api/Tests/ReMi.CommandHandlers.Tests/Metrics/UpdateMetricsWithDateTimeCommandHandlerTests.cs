using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Metrics;
using ReMi.CommandHandlers.Metrics;
using ReMi.Commands.Metrics;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.Events.Metrics;
using ReMi.TestUtils.UnitTests;
using System;
using System.Threading.Tasks;

namespace ReMi.CommandHandlers.Tests.Metrics
{
    public class UpdateMetricsWithDateTimeCommandHandlerTests : TestClassFor<UpdateMetricsWithDateTimeCommandHandler>
    {
        private Mock<IMetricsGateway> _metricsGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override UpdateMetricsWithDateTimeCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateMetricsWithDateTimeCommandHandler
            {
                MetricsGatewayFactory = () => _metricsGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _eventPublisherMock = new Mock<IPublishEvent>(MockBehavior.Strict);
            _metricsGatewayMock = new Mock<IMetricsGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldUpdateMetric_WhenInvoked()
        {
            var command = new UpdateMetricsWithDateTimeCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                MetricType = RandomData.RandomEnum<MetricType>(),
                ExecutedOn = new DateTime(2015, 1, 1),
                CommandContext = new CommandContext { Id = Guid.NewGuid() }
            };
            var metric = new Metric();

            _metricsGatewayMock.Setup(x => x.CreateOrUpdateMetric(command.ReleaseWindowId, command.MetricType, command.ExecutedOn, null));
            _metricsGatewayMock.Setup(x => x.GetMetric(command.ReleaseWindowId, command.MetricType))
                .Returns(metric);
            _eventPublisherMock.Setup(x => x.Publish(It.Is<MetricsUpdatedEvent>(e =>
                e.Metric == metric
                && e.ReleaseWindowId == command.ReleaseWindowId
                && e.Context.ParentId == command.CommandContext.Id)))
                .Returns((Task[]) null);
            _metricsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _metricsGatewayMock.Verify(m => m.CreateOrUpdateMetric(
                It.IsAny<Guid>(), It.IsAny<MetricType>(), It.IsAny<DateTime?>(), It.IsAny<Guid?>()), Times.Once);
            _metricsGatewayMock.Verify(x => x.GetMetric(It.IsAny<Guid>(), It.IsAny<MetricType>()), Times.Once);
            _eventPublisherMock.Verify(e => e.Publish(It.IsAny<IEvent>()), Times.Once);
            _metricsGatewayMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
