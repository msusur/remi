using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.CommandHandlers.Metrics;
using ReMi.Commands.Metrics;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;

namespace ReMi.CommandHandlers.Tests.Metrics
{
    public class CreateReleaseMetricsHandlerTests : TestClassFor<CreateReleaseMetricsHandler>
    {
        private Mock<IMetricsGateway> _metricsGatewayMock;
        private Mock<IReleaseWindowHelper> _releaseWindowHelperMock;

        protected override CreateReleaseMetricsHandler ConstructSystemUnderTest()
        {
            return new CreateReleaseMetricsHandler
            {
                MetricsGatewayFactory = () => _metricsGatewayMock.Object,
                ReleaseWindowHelper = _releaseWindowHelperMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _metricsGatewayMock = new Mock<IMetricsGateway>();
            _releaseWindowHelperMock = new Mock<IReleaseWindowHelper>();
            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(false);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCreateMetrics_WhenReleaseIsScheduledAndRequiresDowntime()
        {
            var createReleaseMetricsCommand = new CreateReleaseMetricsCommand
            {
                ReleaseWindow =
                    new ReleaseWindow
                    {
                        ExternalId = Guid.NewGuid(),
                        ReleaseType = ReleaseType.Scheduled,
                        RequiresDowntime = true
                    }
            };

            Sut.Handle(createReleaseMetricsCommand);

            _metricsGatewayMock.Verify(
                m =>
                    m.CreateMetrics(createReleaseMetricsCommand.ReleaseWindow.ExternalId,
                        It.Is<IEnumerable<MetricType>>(
                            e => e.Contains(MetricType.StartDeploy) && e.Contains(MetricType.FinishDeploy) && e.Contains(MetricType.Complete))));
            _metricsGatewayMock.Verify(
                m =>
                    m.CreateMetrics(createReleaseMetricsCommand.ReleaseWindow.ExternalId,
                        It.Is<IEnumerable<MetricType>>(
                            e => e.Contains(MetricType.SiteDown) && e.Contains(MetricType.SiteUp))));
        }

        [Test]
        public void Handle_ShouldCreateMetrics_WhenMaintenanceReleaseRequiresDowntime()
        {
            var createReleaseMetricsCommand = new CreateReleaseMetricsCommand
            {
                ReleaseWindow =
                    new ReleaseWindow
                    {
                        ExternalId = Guid.NewGuid(),
                        ReleaseType = ReleaseType.Pci,
                        RequiresDowntime = true
                    }
            };

            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(true);

            Sut.Handle(createReleaseMetricsCommand);

            _metricsGatewayMock.Verify(
                            m =>
                                m.CreateMetrics(createReleaseMetricsCommand.ReleaseWindow.ExternalId,
                                    It.Is<IEnumerable<MetricType>>(
                                        e => e.Contains(MetricType.StartRun) && e.Contains(MetricType.FinishRun))));
            _metricsGatewayMock.Verify(
                m =>
                    m.CreateMetrics(createReleaseMetricsCommand.ReleaseWindow.ExternalId,
                        It.Is<IEnumerable<MetricType>>(
                            e => e.Contains(MetricType.SiteDown) && e.Contains(MetricType.SiteUp))));
        }

        [Test]
        public void Handle_ShouldCreateMetrics_WhenReleaseIsScheduledAndDoesNotRequireDowntime()
        {
            var createReleaseMetricsCommand = new CreateReleaseMetricsCommand
            {
                ReleaseWindow =
                    new ReleaseWindow
                    {
                        ExternalId = Guid.NewGuid(),
                        ReleaseType = ReleaseType.Scheduled,
                        RequiresDowntime = false
                    }
            };

            Sut.Handle(createReleaseMetricsCommand);

            _metricsGatewayMock.Verify(
               m =>
                   m.CreateMetrics(createReleaseMetricsCommand.ReleaseWindow.ExternalId,
                       It.Is<IEnumerable<MetricType>>(
                           e => e.Contains(MetricType.StartDeploy) && e.Contains(MetricType.FinishDeploy))));
        }

        [Test]
        public void Handle_ShouldCreateMetrics_WhenReleaseIsAutomated()
        {
            var createReleaseMetricsCommand = new CreateReleaseMetricsCommand
            {
                ReleaseWindow =
                    new ReleaseWindow
                    {
                        ExternalId = Guid.NewGuid(),
                        ReleaseType = ReleaseType.Automated,
                        RequiresDowntime = false
                    }
            };

            Sut.Handle(createReleaseMetricsCommand);

            _metricsGatewayMock.Verify(
               m =>
                   m.CreateMetrics(createReleaseMetricsCommand.ReleaseWindow.ExternalId,
                       It.Is<IEnumerable<MetricType>>(
                           e => e.Contains(MetricType.StartDeploy) && e.Contains(MetricType.FinishDeploy) && !e.Contains(MetricType.Complete))));
        }

        [Test]
        public void Handle_ShouldCreateMetrics_WhenReleaseIsMaintenance()
        {
            var createReleaseMetricsCommand = new CreateReleaseMetricsCommand
            {
                ReleaseWindow =
                    new ReleaseWindow
                    {
                        ExternalId = Guid.NewGuid(),
                        ReleaseType = ReleaseType.SystemMaintenance,
                        RequiresDowntime = false
                    }
            };

            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(true);

            Sut.Handle(createReleaseMetricsCommand);

            _metricsGatewayMock.Verify(
               m =>
                   m.CreateMetrics(createReleaseMetricsCommand.ReleaseWindow.ExternalId,
                       It.Is<IEnumerable<MetricType>>(
                           e => e.Contains(MetricType.StartRun) && e.Contains(MetricType.FinishRun) && !e.Contains(MetricType.Complete))));
        }

        [Test]
        public void Handle_ShouldCreateMetrics_WhenMaintenanceReleaseDoesNotRequireDowntime()
        {
            var createReleaseMetricsCommand = new CreateReleaseMetricsCommand
            {
                ReleaseWindow =
                    new ReleaseWindow
                    {
                        ExternalId = Guid.NewGuid(),
                        ReleaseType = ReleaseType.Pci,
                        RequiresDowntime = false
                    }
            };
            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(true);

            Sut.Handle(createReleaseMetricsCommand);

            _metricsGatewayMock.Verify(
                            m =>
                                m.CreateMetrics(createReleaseMetricsCommand.ReleaseWindow.ExternalId,
                                    It.Is<IEnumerable<MetricType>>(
                                        e => e.Contains(MetricType.StartRun) && e.Contains(MetricType.FinishRun))));
        }
    }
}
