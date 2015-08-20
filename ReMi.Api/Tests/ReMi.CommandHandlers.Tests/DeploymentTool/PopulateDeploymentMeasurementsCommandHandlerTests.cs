using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.Products;
using ReMi.CommandHandlers.DeploymentTool;
using ReMi.Commands.DeploymentTool;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Data.DeploymentTool;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReleaseJob = ReMi.BusinessEntities.DeploymentTool.ReleaseJob;

namespace ReMi.CommandHandlers.Tests.DeploymentTool
{
    [TestFixture]
    public class PopulateDeploymentMeasurementsCommandHandlerTests : TestClassFor<PopulateDeploymentMeasurementsCommandHandler>
    {
        private Mock<IDeploymentTool> _deploymentToolMock;
        private Mock<IReleaseDeploymentMeasurementGateway> _releaseDeploymentMeasurementGatewayMock;
        private Mock<IReleaseJobGateway> _releaseJobGatewayMock;
        private Mock<IMappingEngine> _mappingEngineMock;
        private Mock<IProductGateway> _packageGatewayMock;


        protected override PopulateDeploymentMeasurementsCommandHandler ConstructSystemUnderTest()
        {
            return new PopulateDeploymentMeasurementsCommandHandler
            {
                MappingEngine = _mappingEngineMock.Object,
                DeploymentToolService = _deploymentToolMock.Object,
                ReleaseDeploymentMeasurementGatewayFactory = () => _releaseDeploymentMeasurementGatewayMock.Object,
                ReleaseJobGatewayFactory = () => _releaseJobGatewayMock.Object,
                PackageGatewayFactory = () => _packageGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _deploymentToolMock = new Mock<IDeploymentTool>(MockBehavior.Strict);
            _packageGatewayMock = new Mock<IProductGateway>(MockBehavior.Strict);
            _releaseDeploymentMeasurementGatewayMock = new Mock<IReleaseDeploymentMeasurementGateway>(MockBehavior.Strict);
            _releaseJobGatewayMock = new Mock<IReleaseJobGateway>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldDoNothing_WhenNoReleaseJobs()
        {
            var command = new PopulateDeploymentMeasurementsCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };
            _releaseJobGatewayMock.Setup(x => x.GetReleaseJobs(command.ReleaseWindowId, true))
                .Returns(Enumerable.Empty<ReleaseJob>());
            _releaseJobGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseJobGatewayMock.Verify(x => x.GetReleaseJobs(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
            _packageGatewayMock.Verify(x => x.GetProducts(It.IsAny<Guid>()), Times.Never);
            _deploymentToolMock.Verify(x => x.GetMetrics(It.IsAny<IDictionary<Guid, IDictionary<Guid, int?>>>()), Times.Never);
            _releaseDeploymentMeasurementGatewayMock.Verify(x => x.StoreDeploymentMeasurements(
                It.IsAny<IEnumerable<JobMeasurement>>(), It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Never);
        }


        [Test]
        [ExpectedException(typeof(MoreThanOnePackageAssignToReleaseException))]
        public void Handle_ShouldThrowException_WhenMoreThanOnePackage()
        {
            var command = new PopulateDeploymentMeasurementsCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };
            var jobs = Builder<ReleaseJob>.CreateListOfSize(5).Build();
            var packages = Builder<Product>.CreateListOfSize(5).Build();
            _releaseJobGatewayMock.Setup(x => x.GetReleaseJobs(command.ReleaseWindowId, true))
                .Returns(jobs);
            _packageGatewayMock.Setup(x => x.GetProducts(command.ReleaseWindowId))
                .Returns(packages);

            _releaseJobGatewayMock.Setup(x => x.Dispose());
            _packageGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldDoNothing_WhenNoMeasurements()
        {
            var command = new PopulateDeploymentMeasurementsCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            };
            var jobs = Builder<ReleaseJob>.CreateListOfSize(5).Build();
            var package = Builder<Product>.CreateNew().Build();
            var metrics = Builder<JobMetric>.CreateListOfSize(10).Build();
            var measurements = metrics.Select(x => new JobMeasurement
            {
                StepName = x.Name,
                BuildNumber = x.BuildNumber,
                StepId = x.JobId
            }).ToArray();
            _releaseJobGatewayMock.Setup(x => x.GetReleaseJobs(command.ReleaseWindowId, true))
                .Returns(jobs);
            _packageGatewayMock.Setup(x => x.GetProducts(command.ReleaseWindowId))
                .Returns(new[] { package });
            _deploymentToolMock.Setup(x => x.GetMetrics(It.Is<IDictionary<Guid, IDictionary<Guid, int?>>>(
                p => p.Keys.First() == package.ExternalId && p.Values.First().All(j => jobs.Any(y => y.ExternalId == j.Key)))))
                .Returns(metrics);
            _mappingEngineMock.Setup(x => x.Map<IEnumerable<JobMetric>, IEnumerable<JobMeasurement>>(metrics))
                .Returns(measurements);
            _releaseDeploymentMeasurementGatewayMock.Setup(x => x.StoreDeploymentMeasurements(
                measurements, command.ReleaseWindowId, command.CommandContext.UserId));

            _releaseJobGatewayMock.Setup(x => x.Dispose());
            _packageGatewayMock.Setup(x => x.Dispose());
            _releaseDeploymentMeasurementGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseJobGatewayMock.Verify(x => x.GetReleaseJobs(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
            _packageGatewayMock.Verify(x => x.GetProducts(It.IsAny<Guid>()), Times.Once);
            _deploymentToolMock.Verify(x => x.GetMetrics(It.IsAny<IDictionary<Guid, IDictionary<Guid, int?>>>()), Times.Once);
            _releaseDeploymentMeasurementGatewayMock.Verify(x => x.StoreDeploymentMeasurements(
                It.IsAny<IEnumerable<JobMeasurement>>(), It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Once);
        }
    }
}
