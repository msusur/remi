using System;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.CommandHandlers.Metrics;
using ReMi.Commands.Metrics;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;

namespace ReMi.CommandHandlers.Tests.Metrics
{
    public class UpdateReleaseWindowMetricsTests : TestClassFor<UpdateReleaseMetricsHandler>
    {
        private Mock<IMetricsGateway> _metricGatewayMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;

        protected override void TestInitialize()
        {
            _metricGatewayMock = new Mock<IMetricsGateway>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();

            base.TestInitialize();
        }

        protected override UpdateReleaseMetricsHandler ConstructSystemUnderTest()
        {
           return new UpdateReleaseMetricsHandler
           {
               CommandDispatcher = _commandDispatcherMock.Object,
               MetricsGatewayFactory = () => _metricGatewayMock.Object
           };
        }

        [Test]
        public void Handle_ShoouldCallGatewayToRemoveOldMetrics()
        {
            var command = new UpdateReleaseMetricsCommand
            {
                ReleaseWindow = new ReleaseWindow {ExternalId = Guid.NewGuid()}
            };

            Sut.Handle(command);

            _metricGatewayMock.Verify(m => m.DeleteMetrics(command.ReleaseWindow.ExternalId));
        }

        [Test]
        public void Handle_ShoouldSendCommandToCreateNewMetrics()
        {
            var command = new UpdateReleaseMetricsCommand
            {
                ReleaseWindow = new ReleaseWindow { ExternalId = Guid.NewGuid() }
            };

            Sut.Handle(command);

            _commandDispatcherMock.Verify(
                m => m.Send(It.Is<CreateReleaseMetricsCommand>(c => c.ReleaseWindow == command.ReleaseWindow)));
        }
    }
}
