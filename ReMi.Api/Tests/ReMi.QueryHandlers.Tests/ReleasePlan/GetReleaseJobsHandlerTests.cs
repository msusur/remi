using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryHandlers.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.QueryHandlers.Tests.ReleasePlan
{
    public class GetReleaseJobsHandlerTests : TestClassFor<GetReleaseJobsHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IReleaseJobGateway> _releaseJobGatewayMock;
        private Mock<IProductGateway> _packageGatewayMock;
        private Mock<IDeploymentTool> _deploymentToolServiceMock;
        private Mock<IMappingEngine> _mapperMock;

        protected override GetReleaseJobsHandler ConstructSystemUnderTest()
        {
            return new GetReleaseJobsHandler
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                ReleaseJobGatewayFactory = () => _releaseJobGatewayMock.Object,
                PackageGatewayFactory = () => _packageGatewayMock.Object,
                Mapper = _mapperMock.Object,
                DeploymentToolService = _deploymentToolServiceMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>(MockBehavior.Strict);
            _releaseJobGatewayMock = new Mock<IReleaseJobGateway>(MockBehavior.Strict);
            _packageGatewayMock = new Mock<IProductGateway>(MockBehavior.Strict);
            _deploymentToolServiceMock = new Mock<IDeploymentTool>(MockBehavior.Strict);
            _mapperMock = new Mock<IMappingEngine>(MockBehavior.Strict);


            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetJobsFromDataBaseAndService_WhenReleaseIsOpen()
        {
            var package = Builder<Product>.CreateNew().Build();
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.Products, new[] {package.Description})
                .With(x => x.ClosedOn, null)
                .Build();
            var request = new GetReleaseJobsRequest {ReleaseWindowId = releaseWindow.ExternalId};
            var jobs = Builder<ReleaseJob>.CreateListOfSize(5).Build();
            var serviceJobs = Builder<Contracts.Plugins.Data.DeploymentTool.ReleaseJob>.CreateListOfSize(3)
                .TheFirst(1)
                .Do(x => x.ExternalId = jobs[0].JobId)
                .Do(x => x.Name = jobs[0].Name)
                .All()
                .Do(x => x.IsDisabled = RandomData.RandomBool())
                .Do(x => x.IsIncluded = RandomData.RandomBool())
                .Build();

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(request.ReleaseWindowId, true, false))
                .Returns(releaseWindow);
            _packageGatewayMock.Setup(x => x.GetProducts(releaseWindow.ExternalId))
                .Returns(new[] {package});
            _releaseJobGatewayMock.Setup(x => x.GetReleaseJobs(releaseWindow.ExternalId, false))
                .Returns(jobs);
            _deploymentToolServiceMock.Setup(
                x => x.GetReleaseJobs(It.Is<IEnumerable<Guid>>(j => j.First() == package.ExternalId)))
                .Returns(serviceJobs);
            _mapperMock.Setup(
                x => x.Map<IEnumerable<Contracts.Plugins.Data.DeploymentTool.ReleaseJob>, IEnumerable<ReleaseJob>>(
                    It.Is<IEnumerable<Contracts.Plugins.Data.DeploymentTool.ReleaseJob>>(
                        j => j.SequenceEqual(serviceJobs.Where(s => !s.IsDisabled)))))
                .Returns(
                    (IEnumerable<Contracts.Plugins.Data.DeploymentTool.ReleaseJob> j) =>
                        j.Select(x => new ReleaseJob {JobId = x.ExternalId}));

            _releaseWindowGatewayMock.Setup(x => x.Dispose());
            _packageGatewayMock.Setup(x => x.Dispose());
            _releaseJobGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            CollectionAssert.AreEquivalent(result.ReleaseJobs.Select(x => x.JobId),
                jobs.Select(x => x.JobId)
                    .Concat(
                        serviceJobs.Where(x => !x.IsDisabled && jobs.All(j => j.JobId != x.ExternalId))
                            .Select(x => x.ExternalId)));
            Assert.IsTrue(
                serviceJobs.Where(x => !x.IsDisabled && jobs.All(j => j.JobId != x.ExternalId)).All(x => !x.IsIncluded));

            _releaseWindowGatewayMock.Verify(
                x => x.GetByExternalId(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _packageGatewayMock.Verify(x => x.GetProducts(It.IsAny<Guid>()), Times.Once);
            _releaseJobGatewayMock.Verify(x => x.GetReleaseJobs(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
            _deploymentToolServiceMock.Verify(x => x.GetReleaseJobs(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldGetOnlyJobsFromDataBase_WhenReleaseIsClosed()
        {
            var package = Builder<Product>.CreateNew().Build();
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.Products, new[] { package.Description })
                .With(x => x.ClosedOn, RandomData.RandomDateTime())
                .Build();
            var request = new GetReleaseJobsRequest { ReleaseWindowId = releaseWindow.ExternalId };
            var jobs = Builder<ReleaseJob>.CreateListOfSize(5).Build();

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(request.ReleaseWindowId, true, false))
                .Returns(releaseWindow);
            _packageGatewayMock.Setup(x => x.GetProducts(releaseWindow.ExternalId))
                .Returns(new[] { package });
            _releaseJobGatewayMock.Setup(x => x.GetReleaseJobs(releaseWindow.ExternalId, false))
                .Returns(jobs);

            _releaseWindowGatewayMock.Setup(x => x.Dispose());
            _packageGatewayMock.Setup(x => x.Dispose());
            _releaseJobGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            CollectionAssert.AreEquivalent(result.ReleaseJobs, jobs);

            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _packageGatewayMock.Verify(x => x.GetProducts(It.IsAny<Guid>()), Times.Once);
            _releaseJobGatewayMock.Verify(x => x.GetReleaseJobs(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
            _deploymentToolServiceMock.Verify(x => x.GetReleaseJobs(It.IsAny<IEnumerable<Guid>>()), Times.Never);
        }
    }
}
