using NUnit.Framework;
using ReMi.CommandHandlers.SourceControl;
using ReMi.TestUtils.UnitTests;
using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using ReMi.BusinessEntities.Products;
using ReMi.Commands.SourceControl;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Contracts.Plugins.Services.SourceControl;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.SourceControl
{
    [TestFixture]
    public class LoadReleaseRepositoriesCommandHandlerTests : TestClassFor<LoadReleaseRepositoriesCommandHandler>
    {
        private Mock<IReleaseRepositoryGateway> _releaseRepositoryGateway;
        private Mock<IProductGateway> _packageGateway;
        private Mock<ISourceControl> _sourceControlService;

        protected override LoadReleaseRepositoriesCommandHandler ConstructSystemUnderTest()
        {
            return new LoadReleaseRepositoriesCommandHandler
            {
                SourceControlService = _sourceControlService.Object,
                PackageGatewayFactory = () => _packageGateway.Object,
                ReleaseRepositoryGatewayFactory = () => _releaseRepositoryGateway.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseRepositoryGateway = new Mock<IReleaseRepositoryGateway>(MockBehavior.Strict);
            _packageGateway = new Mock<IProductGateway>(MockBehavior.Strict);
            _sourceControlService = new Mock<ISourceControl>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldDoNothing_WhenNoRepositoriesFromService()
        {
            var command = new LoadReleaseRepositoriesCommand { ReleaseWindowId = Guid.NewGuid() };
            var packages = Builder<Product>.CreateListOfSize(5).Build();
            _packageGateway.Setup(x => x.GetProducts(command.ReleaseWindowId))
                .Returns(packages);
            _sourceControlService.Setup(x => x.GetSourceControlRetrieveMode(
                    It.Is<IEnumerable<Guid>>(p => p.SequenceEqual(packages.Select(p2 => p2.ExternalId)))))
                .Returns(packages.ToDictionary(x => x.ExternalId, x => SourceControlRetrieveMode.RepositoryIdentifier));
            _sourceControlService.Setup(x => x.GetRepositories(
                    It.Is<IEnumerable<Guid>>(p => p.SequenceEqual(packages.Select(p2 => p2.ExternalId)))))
                .Returns((IEnumerable<ReleaseRepository>)null);
            _packageGateway.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseRepositoryGateway.Verify(x => x.RemoveRepositoriesFromRelease(It.IsAny<Guid>()), Times.Never);
            _releaseRepositoryGateway.Verify(x => x.AddRepositoryToRelease(It.IsAny<ReleaseRepository>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldAddRepositories_WhenRepositoriesTakenFromService()
        {
            var command = new LoadReleaseRepositoriesCommand { ReleaseWindowId = Guid.NewGuid() };
            var packages = Builder<Product>.CreateListOfSize(5).Build();
            var repositories = Builder<ReleaseRepository>.CreateListOfSize(5).Build();

            _packageGateway.Setup(x => x.GetProducts(command.ReleaseWindowId))
                .Returns(packages);
            _sourceControlService.Setup(x => x.GetSourceControlRetrieveMode(
                    It.Is<IEnumerable<Guid>>(p => p.SequenceEqual(packages.Select(p2 => p2.ExternalId)))))
                .Returns(packages.ToDictionary(x => x.ExternalId, x => SourceControlRetrieveMode.RepositoryIdentifier));
            _sourceControlService.Setup(x => x.GetRepositories(
                    It.Is<IEnumerable<Guid>>(p => p.SequenceEqual(packages.Select(p2 => p2.ExternalId)))))
                .Returns(repositories);
            _releaseRepositoryGateway.Setup(x => x.RemoveRepositoriesFromRelease(command.ReleaseWindowId));
            _releaseRepositoryGateway.Setup(x => x.AddRepositoryToRelease(
                It.Is<ReleaseRepository>(r => repositories.Where(r2 => !r2.IsDisabled).Contains(r)), command.ReleaseWindowId));
            _packageGateway.Setup(x => x.Dispose());
            _releaseRepositoryGateway.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseRepositoryGateway.Verify(x => x.RemoveRepositoriesFromRelease(It.IsAny<Guid>()), Times.Once);
            _releaseRepositoryGateway.Verify(x => x.AddRepositoryToRelease(
                It.IsAny<ReleaseRepository>(), It.IsAny<Guid>()), Times.Exactly(repositories.Count(r => !r.IsDisabled)));
        }
    }
}
