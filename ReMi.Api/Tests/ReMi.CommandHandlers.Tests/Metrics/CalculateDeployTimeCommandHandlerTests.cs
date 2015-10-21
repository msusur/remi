using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.CommandHandlers.Metrics;
using ReMi.Commands.Metrics;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.ReleaseExecution;

namespace ReMi.CommandHandlers.Tests.Metrics
{
    [TestFixture]
    public class CalculateDeployTimeCommandHandlerTests : TestClassFor<CalculateDeployTimeCommandHandler>
    {
        private Mock<IDeploymentTool> _deploymentToolMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IProductGateway> _packageGatewayMock;
        private Mock<IHandleQuery<GetDeploymentMeasurementsRequest, GetDeploymentMeasurementsResponse>> _getDeploymentMeasurementsActionMock;

        protected override CalculateDeployTimeCommandHandler ConstructSystemUnderTest()
        {
            return new CalculateDeployTimeCommandHandler
            {
                DeploymentTool = _deploymentToolMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                PackageGatewayFactory = () => _packageGatewayMock.Object,
                GetDeploymentMeasurementsAction = _getDeploymentMeasurementsActionMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _deploymentToolMock = new Mock<IDeploymentTool>(MockBehavior.Strict);
            _commandDispatcherMock = new Mock<ICommandDispatcher>(MockBehavior.Strict);
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>(MockBehavior.Strict);
            _packageGatewayMock = new Mock<IProductGateway>(MockBehavior.Strict);
            _getDeploymentMeasurementsActionMock
                = new Mock<IHandleQuery<GetDeploymentMeasurementsRequest, GetDeploymentMeasurementsResponse>>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldDoNothing_WhenDeploymentToolDoesNotAllowToCalculateDeployTime()
        {
            var command = new CalculateDeployTimeCommand { ReleaseWindowId = Guid.NewGuid() };
            var package = new Product { ExternalId = Guid.NewGuid() };

            _packageGatewayMock.Setup(x => x.GetProducts(command.ReleaseWindowId))
                .Returns(new[] { package });
            _deploymentToolMock.Setup(x => x.AllowGettingDeployTime(package.ExternalId))
                .Returns(false);
            _packageGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _packageGatewayMock.Verify(x => x.GetProducts(It.IsAny<Guid>()), Times.Once);
            _deploymentToolMock.Verify(x => x.AllowGettingDeployTime(It.IsAny<Guid>()), Times.Once);

            _packageGatewayMock.Verify(x => x.Dispose(), Times.Once);
            _releaseWindowGatewayMock.Verify(x => x.Dispose(), Times.Never);
        }

        [Test, TestCaseSource("TestCases")]
        public TestResult Handle_ShouldRunTestFromTestCase_WhenCalled(ReleaseWindow releaseWindow, IEnumerable<JobMeasurement> jobMeasurements)
        {
            var command = new CalculateDeployTimeCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext { Id = Guid.NewGuid() }
            };
            var package = new Product { ExternalId = Guid.NewGuid() };

            DateTime? startTime = null, finishTime = null;

            _packageGatewayMock.Setup(x => x.GetProducts(command.ReleaseWindowId))
                .Returns(new[] { package });
            _deploymentToolMock.Setup(x => x.AllowGettingDeployTime(package.ExternalId))
                .Returns(true);
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(command.ReleaseWindowId, false, false))
                .Returns(releaseWindow);
            _getDeploymentMeasurementsActionMock.Setup(x => x.Handle(It.Is<GetDeploymentMeasurementsRequest>(q =>
                q.Context.ParentId == command.CommandContext.Id
                && q.ReleaseWindowId == command.ReleaseWindowId)))
                .Returns(new GetDeploymentMeasurementsResponse { Measurements = jobMeasurements });
            _commandDispatcherMock.Setup(x => x.Send(It.Is<UpdateMetricsWithDateTimeCommand>(c =>
                c.MetricType == MetricType.StartDeploy
                && c.ReleaseWindowId == command.ReleaseWindowId
                && c.CommandContext.ParentId == command.CommandContext.Id)))
                .Callback((UpdateMetricsWithDateTimeCommand c) => startTime = c.ExecutedOn)
                .Returns((Task)null);
            _commandDispatcherMock.Setup(x => x.Send(It.Is<UpdateMetricsWithDateTimeCommand>(c =>
                c.MetricType == MetricType.FinishDeploy
                && c.ReleaseWindowId == command.ReleaseWindowId
                && c.CommandContext.ParentId == command.CommandContext.Id)))
                .Callback((UpdateMetricsWithDateTimeCommand c) => finishTime = c.ExecutedOn)
                .Returns((Task)null);

            _packageGatewayMock.Setup(x => x.Dispose());
            _releaseWindowGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _packageGatewayMock.Verify(x => x.GetProducts(It.IsAny<Guid>()), Times.Once);
            _deploymentToolMock.Verify(x => x.AllowGettingDeployTime(It.IsAny<Guid>()), Times.Once);

            _packageGatewayMock.Verify(x => x.Dispose(), Times.Once);
            _releaseWindowGatewayMock.Verify(x => x.Dispose(), Times.Once);

            return new TestResult(startTime, finishTime);
        }

        private static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                yield return new TestCaseData(new ReleaseWindow { RequiresDowntime = false },
                    new[]
                    {
                        new JobMeasurement
                        {
                            StartTime = new DateTime(2015, 1, 1, 22, 0, 0),
                            FinishTime = new DateTime(2015, 1, 1, 22, 30, 0)
                        }
                    })
                    .Returns(new TestResult(new DateTime(2015, 1, 1, 22, 0, 0), new DateTime(2015, 1, 1, 22, 30, 0)))
                    .SetName("TestCase1_WithoutSiteDown")
                    .SetDescription("ShouldCalculateDeployTimeBasedOnStartAndFinishTime_WhenOnlyOneJobExists");

                yield return new TestCaseData(new ReleaseWindow { RequiresDowntime = false },
                    new[]
                    {
                        new JobMeasurement
                        {
                            StartTime = new DateTime(2015, 1, 1, 22, 0, 0),
                            FinishTime = new DateTime(2015, 1, 1, 22, 30, 0)
                        },
                        new JobMeasurement
                        {
                            StartTime = new DateTime(2015, 1, 1, 22, 10, 0),
                            FinishTime = new DateTime(2015, 1, 1, 22, 35, 43)
                        },
                        new JobMeasurement
                        {
                            StartTime = new DateTime(2015, 1, 1, 21, 59, 24),
                            FinishTime = new DateTime(2015, 1, 1, 22, 32, 3)
                        }
                    })
                    .Returns(new TestResult(new DateTime(2015, 1, 1, 21, 59, 24), new DateTime(2015, 1, 1, 22, 35, 43)))
                    .SetName("TestCase2_WithoutSiteDown")
                    .SetDescription("ShouldCalculateDeployTimeBasedOnStartAndFinishTime_WhenMoreThenOneJobExists");

                yield return new TestCaseData(new ReleaseWindow { RequiresDowntime = false },
                    new JobMeasurement[0])
                    .Returns(new TestResult(null, null))
                    .SetName("TestCase3_WithoutSiteDown")
                    .SetDescription("ShouldNotCalculateDeployTime_WhenJobMeasurementsNotExists");


                yield return new TestCaseData(new ReleaseWindow { RequiresDowntime = true },
                    new[]
                    {
                        new JobMeasurement
                        {
                            StartTime = new DateTime(2015, 1, 1, 22, 0, 0),
                            FinishTime = new DateTime(2015, 1, 1, 22, 29, 0),
                            JobStage = JobStage.DuringOffline
                        },
                        new JobMeasurement
                        {
                            StartTime = new DateTime(2015, 1, 1, 22, 10, 0),
                            FinishTime = new DateTime(2015, 1, 1, 22, 35, 43),
                            JobStage = JobStage.AfterOffline
                        },
                        new JobMeasurement
                        {
                            StartTime = new DateTime(2015, 1, 1, 21, 59, 24),
                            FinishTime = new DateTime(2015, 1, 1, 22, 32, 3),
                            JobStage = JobStage.AfterOffline
                        }
                    })
                    .Returns(new TestResult(new DateTime(2015, 1, 1, 22, 0, 0), new DateTime(2015, 1, 1, 22, 29, 0)))
                    .SetName("TestCase4_WithSiteDown")
                    .SetDescription("ShouldCalculateDeployTimeBasedOnStartAndFinishTime_WhenJobsWhereTriggeredOffline");

                yield return new TestCaseData(new ReleaseWindow { RequiresDowntime = true },
                    new[]
                    {
                        new JobMeasurement
                        {
                            StartTime = new DateTime(2015, 1, 1, 22, 10, 0),
                            FinishTime = new DateTime(2015, 1, 1, 22, 35, 43),
                            JobStage = JobStage.AfterOffline
                        },
                        new JobMeasurement
                        {
                            StartTime = new DateTime(2015, 1, 1, 21, 59, 24),
                            FinishTime = new DateTime(2015, 1, 1, 22, 32, 3),
                            JobStage = JobStage.AfterOffline
                        }
                    })
                    .Returns(new TestResult(null, null))
                    .SetName("TestCase5_WithSiteDown")
                    .SetDescription("ShouldNotCalculateDeployTime_WhenNoJobsTriggeredDuringOffline");
            }
        }

        public class TestResult
        {
            private readonly DateTime? _startDeploy;
            private readonly DateTime? _finishDeploy;

            public TestResult(DateTime? startDeploy, DateTime? finishDeploy)
            {
                _startDeploy = startDeploy;
                _finishDeploy = finishDeploy;
            }

            public override string ToString()
            {
                return string.Format("[{0:dd/MM/yy HH:mm:ss} - {1:dd/MM/yy HH:mm:ss}]", _startDeploy, _finishDeploy);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((TestResult)obj);
            }

            private bool Equals(TestResult other)
            {
                return _startDeploy.Equals(other._startDeploy)
                    && _finishDeploy.Equals(other._finishDeploy);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_startDeploy.GetHashCode() * 397) ^ _finishDeploy.GetHashCode();
                }
            }
        }
    }
}
