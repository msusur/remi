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
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Contracts.Plugins.Services.SourceControl;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;
using ReMi.DataAccess.Exceptions;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryHandlers.ReleasePlan;

namespace ReMi.QueryHandlers.Tests.ReleasePlan
{
    public class GetReleaseChangesHandlerTests : TestClassFor<GetReleaseChangesHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<ISourceControlChangeGateway> _changesGatewayMock;
        private Mock<IReleaseJobGateway> _releaseJobGatewayMock;
        private Mock<IReleaseRepositoryGateway> _releaseRepositoryGatewayMock;
        private Mock<IProductGateway> _packageGatewayMock;
        private Mock<IMappingEngine> _mappingEngineMock;
        private Mock<ISourceControl> _sourceControlMock;

        protected override GetReleaseChangesHandler ConstructSystemUnderTest()
        {
            return new GetReleaseChangesHandler
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                MappingEngine = _mappingEngineMock.Object,
                SourceControlChangeGatewayFactory = () => _changesGatewayMock.Object,
                SourceControlService = _sourceControlMock.Object,
                ProductGatewayFactory = () => _packageGatewayMock.Object,
                ReleaseJobGatewayFactory = () => _releaseJobGatewayMock.Object,
                ReleaseRepositoryGatewayFactory = () => _releaseRepositoryGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>(MockBehavior.Strict);
            _sourceControlMock = new Mock<ISourceControl>(MockBehavior.Strict);
            _packageGatewayMock = new Mock<IProductGateway>(MockBehavior.Strict);
            _changesGatewayMock = new Mock<ISourceControlChangeGateway>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);
            _releaseJobGatewayMock = new Mock<IReleaseJobGateway>(MockBehavior.Strict);
            _releaseRepositoryGatewayMock = new Mock<IReleaseRepositoryGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldThrowException_WhenNoPackageAssignToReleaseWindow()
        {
            var request = new GetReleaseChangesRequest { ReleaseWindowId = Guid.NewGuid() };
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, request.ReleaseWindowId)
                .Build();

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(request.ReleaseWindowId, true, false))
                .Returns(releaseWindow);
            _releaseWindowGatewayMock.Setup(x => x.Dispose());

            var ex = Assert.Throws<ProductShouldBeAssignedException>(() => Sut.Handle(request));

            Assert.IsTrue(ex.Message.Contains(request.ReleaseWindowId.ToString()));
        }

        [Test]
        public void Handle_ShouldGetSourceCodeChangesFromDataBase_WhenReleaseIsApprovedAndRequestIsNotBackground()
        {
            var request = new GetReleaseChangesRequest { ReleaseWindowId = Guid.NewGuid() };
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, request.ReleaseWindowId)
                .With(x => x.ApprovedOn, RandomData.RandomDateTime())
                .With(x => x.Products, new[] { "product" })
                .Build();
            var changes = Builder<SourceControlChange>.CreateListOfSize(5).Build();

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(request.ReleaseWindowId, true, false))
                .Returns(releaseWindow);
            _changesGatewayMock.Setup(x => x.GetChanges(releaseWindow.ExternalId))
                .Returns(changes);

            _releaseWindowGatewayMock.Setup(x => x.Dispose());
            _changesGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            CollectionAssert.AreEquivalent(changes, result.Changes);

            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _changesGatewayMock.Verify(x => x.GetChanges(It.IsAny<Guid>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldGetSourceCodeChangesFromServiceWhichaAreNotYetInDataBase_WhenReleaseNotApprovedAndRequestIsNotBackground()
        {
            var package = Builder<Product>.CreateNew().Build();
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.Products, new[] { package.Description })
                .With(x => x.ApprovedOn, null)
                .Build();
            var jobs = Builder<ReleaseJob>.CreateListOfSize(5).Build();
            var request = new GetReleaseChangesRequest { ReleaseWindowId = releaseWindow.ExternalId };
            var changes = Builder<SourceControlChange>.CreateListOfSize(5).Build();

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(request.ReleaseWindowId, true, false))
                .Returns(releaseWindow);
            _packageGatewayMock.Setup(x => x.GetProducts(releaseWindow.ExternalId))
                .Returns(new[] { package });
            _releaseJobGatewayMock.Setup(x => x.GetReleaseJobs(releaseWindow.ExternalId, false))
                .Returns(jobs);
            _sourceControlMock.Setup(x => x.GetSourceControlRetrieveMode(
                It.Is<IEnumerable<Guid>>(i => i.Contains(package.ExternalId))))
                .Returns(new Dictionary<Guid, SourceControlRetrieveMode>
                {
                    { package.ExternalId, SourceControlRetrieveMode.DeploymentJobs }
                });
            _sourceControlMock.Setup(x => x.GetChangesByReleaseJobs(
                    It.Is<IEnumerable<Guid>>(p => p.SequenceEqual(new[] { package.ExternalId })),
                    It.Is<IEnumerable<Guid>>(p => p.SequenceEqual(jobs.Select(j => j.JobId)))))
                .Returns(changes);
            _changesGatewayMock.Setup(x => x.FilterExistingChangesByProduct(
                    It.Is<IEnumerable<string>>(p => p.SequenceEqual(changes.Select(c => c.Identifier))),
                    It.Is<IEnumerable<Guid>>(p => p.SequenceEqual(new[] { package.ExternalId }))))
                .Returns(changes.Take(3).Select(x => x.Identifier).ToArray());

            _releaseWindowGatewayMock.Setup(x => x.Dispose());
            _packageGatewayMock.Setup(x => x.Dispose());
            _releaseJobGatewayMock.Setup(x => x.Dispose());
            _changesGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            CollectionAssert.AreEquivalent(changes.Skip(3), result.Changes);

            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _packageGatewayMock.Verify(x => x.GetProducts(It.IsAny<Guid>()), Times.Once);
            _releaseJobGatewayMock.Verify(x => x.GetReleaseJobs(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
            _sourceControlMock.Verify(x => x.GetChangesByReleaseJobs(It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<Guid>>()), Times.Once);
            _changesGatewayMock.Verify(x => x.FilterExistingChangesByProduct(
                It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Guid>>()), Times.Once);
            _changesGatewayMock.Verify(x => x.GetChanges(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldGetSourceCodeChangesFromServiceWithoutFiltering_WhenReleaseNotApprovedAndRequestIsBackground()
        {
            var package = Builder<Product>.CreateNew().Build();
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.Products, new[] { package.Description })
                .With(x => x.ApprovedOn, null)
                .Build();
            var jobs = Builder<ReleaseJob>.CreateListOfSize(5).Build();
            var request = new GetReleaseChangesRequest { ReleaseWindowId = releaseWindow.ExternalId, IsBackground = true };
            var changes = Builder<SourceControlChange>.CreateListOfSize(5).Build();

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(request.ReleaseWindowId, true, false))
                .Returns(releaseWindow);
            _packageGatewayMock.Setup(x => x.GetProducts(releaseWindow.ExternalId))
                .Returns(new[] { package });
            _releaseJobGatewayMock.Setup(x => x.GetReleaseJobs(releaseWindow.ExternalId, false))
                .Returns(jobs);
            _sourceControlMock.Setup(x => x.GetSourceControlRetrieveMode(
                It.Is<IEnumerable<Guid>>(i => i.Contains(package.ExternalId))))
                .Returns(new Dictionary<Guid, SourceControlRetrieveMode>
                {
                    { package.ExternalId, SourceControlRetrieveMode.DeploymentJobs }
                });
            _sourceControlMock.Setup(x => x.GetChangesByReleaseJobs(
                    It.Is<IEnumerable<Guid>>(p => p.SequenceEqual(new[] { package.ExternalId })),
                    It.Is<IEnumerable<Guid>>(p => p.SequenceEqual(jobs.Select(j => j.JobId)))))
                .Returns(changes);


            _releaseWindowGatewayMock.Setup(x => x.Dispose());
            _packageGatewayMock.Setup(x => x.Dispose());
            _releaseJobGatewayMock.Setup(x => x.Dispose());
            _changesGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            CollectionAssert.AreEquivalent(changes, result.Changes);

            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _packageGatewayMock.Verify(x => x.GetProducts(It.IsAny<Guid>()), Times.Once);
            _releaseJobGatewayMock.Verify(x => x.GetReleaseJobs(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
            _sourceControlMock.Verify(x => x.GetChangesByReleaseJobs(It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<Guid>>()), Times.Once);
            _changesGatewayMock.Verify(x => x.FilterExistingChangesByProduct(
                It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
            _changesGatewayMock.Verify(x => x.GetChanges(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldGetSourceCodeChangesByRepositoriesFromServiceWithoutFiltering_WhenReleaseNotApprovedAndRequestIsBackground()
        {
            var package = Builder<Product>.CreateNew().Build();
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.Products, new[] { package.Description })
                .With(x => x.ApprovedOn, null)
                .Build();
            var repositories = Builder<ReleaseRepository>.CreateListOfSize(5).Build();
            var request = new GetReleaseChangesRequest { ReleaseWindowId = releaseWindow.ExternalId, IsBackground = true };
            var changes = Builder<SourceControlChange>.CreateListOfSize(5).Build();

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(request.ReleaseWindowId, true, false))
                .Returns(releaseWindow);
            _packageGatewayMock.Setup(x => x.GetProducts(releaseWindow.ExternalId))
                .Returns(new[] { package });
            _releaseRepositoryGatewayMock.Setup(x => x.GetReleaseRepositories(request.ReleaseWindowId))
                .Returns(repositories);
            _sourceControlMock.Setup(x => x.GetSourceControlRetrieveMode(
                It.Is<IEnumerable<Guid>>(i => i.Contains(package.ExternalId))))
                .Returns(new Dictionary<Guid, SourceControlRetrieveMode>
                {
                    { package.ExternalId, SourceControlRetrieveMode.RepositoryIdentifier }
                });
            _sourceControlMock.Setup(x => x.GetChangesByRepository(
                    It.Is<IEnumerable<Guid>>(p => p.SequenceEqual(new[] { package.ExternalId })),
                    It.Is<IEnumerable<ReleaseRepository>>(p => p.SequenceEqual(repositories.Where(r => r.IsIncluded)))))
                .Returns(changes);


            _releaseRepositoryGatewayMock.Setup(x => x.Dispose());
            _releaseWindowGatewayMock.Setup(x => x.Dispose());
            _packageGatewayMock.Setup(x => x.Dispose());
            _releaseJobGatewayMock.Setup(x => x.Dispose());
            _changesGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            CollectionAssert.AreEquivalent(changes, result.Changes);

            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _packageGatewayMock.Verify(x => x.GetProducts(It.IsAny<Guid>()), Times.Once);
            _releaseRepositoryGatewayMock.Verify(x => x.GetReleaseRepositories(It.IsAny<Guid>()), Times.Once);
            _releaseJobGatewayMock.Verify(x => x.GetReleaseJobs(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
            _sourceControlMock.Verify(x => x.GetChangesByRepository(It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<ReleaseRepository>>()), Times.Once);
            _sourceControlMock.Verify(x => x.GetChangesByReleaseJobs(It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
            _changesGatewayMock.Verify(x => x.FilterExistingChangesByProduct(
                It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
            _changesGatewayMock.Verify(x => x.GetChanges(It.IsAny<Guid>()), Times.Never);
        }
    }
}
