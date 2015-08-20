using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.EventHandlers.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.Tests.ReleaseCalendar
{
    public class ReleaseWindowBookedEventHandlerAddDeploymentJobTests : TestClassFor<ReleaseWindowBookedEventHandlerAddDeploymentJob>
    {
        private Mock<IProductGateway> _packageGatewayMock;
        private Mock<IReleaseJobGateway> _releaseJobGatewayMock;
        private Mock<IReleaseWindowHelper> _releaseWindowHelperMock;
        private Mock<IDeploymentTool> _deploymentToolMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override ReleaseWindowBookedEventHandlerAddDeploymentJob ConstructSystemUnderTest()
        {
            return new ReleaseWindowBookedEventHandlerAddDeploymentJob
            {
                PackageGatewayFactory = () => _packageGatewayMock.Object,
                ReleaseJobGatewayFactory = () => _releaseJobGatewayMock.Object,
                ReleaseWindowHelper = _releaseWindowHelperMock.Object,
                DeploymentToolService = _deploymentToolMock.Object,
                Mapper = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseJobGatewayMock = new Mock<IReleaseJobGateway>(MockBehavior.Strict);
            _packageGatewayMock = new Mock<IProductGateway>(MockBehavior.Strict);
            _deploymentToolMock = new Mock<IDeploymentTool>(MockBehavior.Strict);
            _releaseWindowHelperMock = new Mock<IReleaseWindowHelper>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldDoNothing_WhenReleaseIsMaintenance()
        {
            var evnt = new ReleaseWindowBookedEvent
            {
                ReleaseWindow = new ReleaseWindow { ReleaseType = ReleaseType.SystemMaintenance }
            };

            _releaseWindowHelperMock.Setup(o => o.IsMaintenance(evnt.ReleaseWindow))
                .Returns(true);

            Sut.Handle(evnt);

            _releaseWindowHelperMock.Verify(o => o.IsMaintenance(It.IsAny<ReleaseWindow>()), Times.Once);
            _packageGatewayMock.Verify(o => o.GetProducts(It.IsAny<IEnumerable<string>>()), Times.Never);
            _deploymentToolMock.Verify(o => o.GetReleaseJobs(It.IsAny<IEnumerable<Guid>>()), Times.Never);
            _releaseJobGatewayMock.Verify(o => o.GetReleaseJobs(It.IsAny<Guid>(), false), Times.Never);
            _releaseJobGatewayMock.Verify(o => o.AddJobToRelease(It.IsAny<ReleaseJob>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldNotAddDeploymentJobsToRelease_WhenThereIsNoDeploymentJobs()
        {
            var evnt = new ReleaseWindowBookedEvent
            {
                ReleaseWindow = new ReleaseWindow
                {
                    ReleaseType = ReleaseType.Scheduled,
                    ExternalId = Guid.NewGuid()
                }
            };
            var packages = Builder<Product>.CreateListOfSize(1).Build();
            var jobs = Enumerable.Empty<Contracts.Plugins.Data.DeploymentTool.ReleaseJob>();
            var businessJobs = Enumerable.Empty<ReleaseJob>();

            _releaseWindowHelperMock.Setup(o => o.IsMaintenance(evnt.ReleaseWindow))
                .Returns(false);
            _packageGatewayMock.Setup(o => o.GetProducts(evnt.ReleaseWindow.ExternalId))
                .Returns(packages);
            _deploymentToolMock.Setup(x => x.GetReleaseJobs(
                    It.Is<IEnumerable<Guid>>(j => j.SequenceEqual(packages.Select(p => p.ExternalId)))))
                .Returns(jobs);
            _mappingEngineMock.Setup(
                x => x.Map<IEnumerable<Contracts.Plugins.Data.DeploymentTool.ReleaseJob>, IEnumerable<ReleaseJob>>(jobs))
                .Returns(businessJobs);

            _packageGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(evnt);

            _releaseWindowHelperMock.Verify(o => o.IsMaintenance(It.IsAny<ReleaseWindow>()), Times.Once);
            _packageGatewayMock.Verify(o => o.GetProducts(It.IsAny<Guid>()), Times.Once);
            _deploymentToolMock.Verify(o => o.GetReleaseJobs(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            _releaseJobGatewayMock.Verify(o => o.GetReleaseJobs(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
            _releaseJobGatewayMock.Verify(o => o.AddJobToRelease(It.IsAny<ReleaseJob>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldAddDeploymentJobsToRelease_WhenDeploymentJobsExist()
        {
            var evnt = new ReleaseWindowBookedEvent
            {
                ReleaseWindow = new ReleaseWindow
                {
                    ReleaseType = ReleaseType.Scheduled,
                    ExternalId = Guid.NewGuid()
                }
            };
            var packages = Builder<Product>.CreateListOfSize(1).Build();
            var jobs = Builder<Contracts.Plugins.Data.DeploymentTool.ReleaseJob>.CreateListOfSize(10)
                .All()
                .Do(x => x.IsDisabled = RandomData.RandomBool())
                .Build();
            var businessJobs = jobs.Where(x => !x.IsDisabled)
                .Select(x => new ReleaseJob { JobId = Guid.NewGuid(), IsIncluded = true }).ToArray();
            var dataJobs = Builder<ReleaseJob>.CreateListOfSize(2).Build();

            _releaseWindowHelperMock.Setup(o => o.IsMaintenance(evnt.ReleaseWindow))
                .Returns(false);
            _packageGatewayMock.Setup(o => o.GetProducts(evnt.ReleaseWindow.ExternalId))
                .Returns(packages);
            _deploymentToolMock.Setup(x => x.GetReleaseJobs(
                    It.Is<IEnumerable<Guid>>(j => j.SequenceEqual(packages.Select(p => p.ExternalId)))))
                .Returns(jobs);
            _mappingEngineMock.Setup(
                x => x.Map<IEnumerable<Contracts.Plugins.Data.DeploymentTool.ReleaseJob>, IEnumerable<ReleaseJob>>(
                    It.Is<IEnumerable<Contracts.Plugins.Data.DeploymentTool.ReleaseJob>>(j => j.SequenceEqual(jobs.Where(z => !z.IsDisabled)))))
                .Returns(businessJobs);
            _releaseJobGatewayMock.Setup(x => x.GetReleaseJobs(evnt.ReleaseWindow.ExternalId, false))
                .Returns(dataJobs);
            businessJobs.Each(x => _releaseJobGatewayMock.Setup(o => o.AddJobToRelease(x, evnt.ReleaseWindow.ExternalId)));

            _packageGatewayMock.Setup(x => x.Dispose());
            _releaseJobGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(evnt);

            _releaseWindowHelperMock.Verify(o => o.IsMaintenance(It.IsAny<ReleaseWindow>()), Times.Once);
            _packageGatewayMock.Verify(o => o.GetProducts(It.IsAny<Guid>()), Times.Once);
            _deploymentToolMock.Verify(o => o.GetReleaseJobs(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            _releaseJobGatewayMock.Verify(o => o.GetReleaseJobs(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
            _releaseJobGatewayMock.Verify(o => o.AddJobToRelease(It.IsAny<ReleaseJob>(), It.IsAny<Guid>()),
                Times.Exactly(businessJobs.Count()));
        }
    }
}
