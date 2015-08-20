using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using ReMi.DataEntities.SourceControl;

namespace ReMi.DataAccess.Tests.ReleasePlan
{
    [TestFixture]
    public class ReleaseRepositoryGatewayTests : TestClassFor<ReleaseRepositoryGateway>
    {
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepository;
        private Mock<IRepository<ReleaseRepository>> _releaseRepository;
        private Mock<IRepository<SourceControlChange>> _sourceControlChangeRepository;
        private Mock<IMappingEngine> _mappingEngine;

        protected override ReleaseRepositoryGateway ConstructSystemUnderTest()
        {
            return new ReleaseRepositoryGateway
            {
                ReleaseRepository = _releaseRepository.Object,
                MappingEngine = _mappingEngine.Object,
                ReleaseWindowRepository = _releaseWindowRepository.Object,
                SourceControlChangeRepository = _sourceControlChangeRepository.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowRepository = new Mock<IRepository<ReleaseWindow>>(MockBehavior.Strict);
            _releaseRepository = new Mock<IRepository<ReleaseRepository>>(MockBehavior.Strict);
            _sourceControlChangeRepository = new Mock<IRepository<SourceControlChange>>(MockBehavior.Strict);
            _mappingEngine = new Mock<IMappingEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Dispose_ShouldDisposeAllRepositories_WhenCalled()
        {
            _releaseWindowRepository.Setup(x => x.Dispose());
            _releaseRepository.Setup(x => x.Dispose());
            _sourceControlChangeRepository.Setup(x => x.Dispose());

            Sut.Dispose();

            _releaseWindowRepository.Verify(x => x.Dispose(), Times.Once);
            _releaseRepository.Verify(x => x.Dispose(), Times.Once);
            _sourceControlChangeRepository.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void GetReleaseRepository_ShouldThrowException_WhenReleaseWindowIsNotFound()
        {
            _releaseWindowRepository.SetupEntities(Enumerable.Empty<ReleaseWindow>());
            var releaseWindowId = Guid.NewGuid();

            var ex = Assert.Throws<ReleaseWindowNotFoundException>(() => Sut.GetReleaseRepository(releaseWindowId, Guid.NewGuid()));

            Assert.IsTrue(ex.Message.Contains(releaseWindowId.ToString()));
        }

        [Test]
        public void GetReleaseRepository_ShouldGetNull_WhenRepositoryDoesNotExist()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew().Build();
            
            _releaseWindowRepository.SetupEntities(new[] { releaseWindow });
            _releaseRepository.SetupEntities(Enumerable.Empty<ReleaseRepository>());

            var result = Sut.GetReleaseRepository(releaseWindow.ExternalId, Guid.NewGuid());

            Assert.IsNull(result);
        }

        [Test]
        public void GetReleaseRepository_ShouldGetMappedReleaseRepository_WhenExists()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew().Build();
            var dataRepository = Builder<ReleaseRepository>.CreateNew()
                .With(x => x.ReleaseWindowId, releaseWindow.ReleaseWindowId)
                .Build();
            var contractRepository = new Contracts.Plugins.Data.SourceControl.ReleaseRepository
            {
                ExternalId = dataRepository.RepositoryId
            };

            _releaseWindowRepository.SetupEntities(new[] { releaseWindow });
            _releaseRepository.SetupEntities(new[] { dataRepository });
            _mappingEngine.Setup(
                x => x.Map<ReleaseRepository, Contracts.Plugins.Data.SourceControl.ReleaseRepository>(dataRepository))
                .Returns(contractRepository);

            var result = Sut.GetReleaseRepository(releaseWindow.ExternalId, contractRepository.ExternalId);

            Assert.AreEqual(contractRepository, result);
        }

        [Test]
        public void GetReleaseRepositories_ShouldThrowException_WhenReleaseWindowIsNotFound()
        {
            _releaseWindowRepository.SetupEntities(Enumerable.Empty<ReleaseWindow>());
            var releaseWindowId = Guid.NewGuid();

            var ex = Assert.Throws<ReleaseWindowNotFoundException>(() => Sut.GetReleaseRepositories(releaseWindowId));

            Assert.IsTrue(ex.Message.Contains(releaseWindowId.ToString()));
        }

        [Test]
        public void GetReleaseRepositories_ShouldGetMappedReleaseRepositoriesWithLatestChanges_WhenCalledAndLatestChangesExist()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew().Build();
            var dataRepositories = Builder<ReleaseRepository>.CreateListOfSize(5)
                .All()
                .With(x => x.ChangesFrom, null)
                .With(x => x.ReleaseWindowId, releaseWindow.ReleaseWindowId)
                .Do(x => x.LatestChange = RandomData.RandomBool())
                .Build();

            var contractRepositories = dataRepositories
                .Select(x => new Contracts.Plugins.Data.SourceControl.ReleaseRepository
                {
                    ExternalId = x.RepositoryId,
                    LatestChange = x.LatestChange,
                    Repository = x.Name
                }).ToArray();
            var changes = Builder<SourceControlChange>.CreateListOfSize(20).Build();
            var repo = contractRepositories.FirstOrDefault(x => x.LatestChange);
            if (repo != null)
                RandomData.RandomElement(changes).Repository = repo.Repository;

            _releaseWindowRepository.SetupEntities(new[] { releaseWindow });
            _releaseRepository.SetupEntities(dataRepositories);
            _mappingEngine.Setup(
                x => x.Map<IEnumerable<ReleaseRepository>, IEnumerable<Contracts.Plugins.Data.SourceControl.ReleaseRepository>>(
                    It.Is<IEnumerable<ReleaseRepository>>(r => r.SequenceEqual(dataRepositories))))
                .Returns(contractRepositories);
            _sourceControlChangeRepository.SetupEntities(changes);

            var result = Sut.GetReleaseRepositories(releaseWindow.ExternalId);

            CollectionAssert.AreEquivalent(contractRepositories, result);
            if (repo != null)
                Assert.IsNotNullOrEmpty(repo.ChangesFrom);
        }

        [Test]
        public void AddRepositoryToRelease_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            _releaseWindowRepository.SetupEntities(Enumerable.Empty<ReleaseWindow>());
            var releaseWindowId = Guid.NewGuid();

            var ex = Assert.Throws<ReleaseWindowNotFoundException>(() => Sut.AddRepositoryToRelease(null, releaseWindowId));

            Assert.IsTrue(ex.Message.Contains(releaseWindowId.ToString()));
        }

        [Test]
        public void AddRepositoryToRelease_ShouldThrowException_WhenRepositoryAlreadyExists()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew().Build();
            var repo = Builder<Contracts.Plugins.Data.SourceControl.ReleaseRepository>.CreateNew().Build();
            var dataRepo = Builder<ReleaseRepository>.CreateNew().Build();

            _releaseWindowRepository.SetupEntities(new[] { releaseWindow });
            _releaseRepository.SetupEntities(new[] { dataRepo });
            var ex = Assert.Throws<EntityAlreadyExistsException>(() => Sut.AddRepositoryToRelease(repo, releaseWindow.ExternalId));

            Assert.IsTrue(ex.Message.Contains(releaseWindow.ExternalId.ToString()));
            Assert.IsTrue(ex.Message.Contains(repo.ExternalId.ToString()));
        }

        [Test]
        public void AddRepositoryToRelease_ShouldInsertNewRepository_WhenSucced()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ReleaseWindowId, RandomData.RandomInt(int.MaxValue))
                .Build();
            var repo = Builder<Contracts.Plugins.Data.SourceControl.ReleaseRepository>.CreateNew().Build();
            var dataRepo = Builder<ReleaseRepository>.CreateNew().Build();

            _releaseWindowRepository.SetupEntities(new[] { releaseWindow });
            _releaseRepository.SetupEntities(Enumerable.Empty<ReleaseRepository>());
            _mappingEngine.Setup(
                x => x.Map<Contracts.Plugins.Data.SourceControl.ReleaseRepository, ReleaseRepository>(repo))
                .Returns(dataRepo);
            _releaseRepository.Setup(x => x.Insert(dataRepo));

            Sut.AddRepositoryToRelease(repo, releaseWindow.ExternalId);

            Assert.AreEqual(releaseWindow.ReleaseWindowId, dataRepo.ReleaseWindowId);
            _releaseRepository.Verify(x => x.Insert(It.IsAny<ReleaseRepository>()), Times.Once);
        }

        [Test]
        public void UpdateRepository_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            _releaseWindowRepository.SetupEntities(Enumerable.Empty<ReleaseWindow>());
            var releaseWindowId = Guid.NewGuid();

            var ex = Assert.Throws<ReleaseWindowNotFoundException>(() => Sut.UpdateRepository(null, releaseWindowId));

            Assert.IsTrue(ex.Message.Contains(releaseWindowId.ToString()));
        }

        [Test]
        public void UpdateRepository_ShouldThrowException_WhenRepositoryNotFound()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew().Build();
            var repo = Builder<Contracts.Plugins.Data.SourceControl.ReleaseRepository>.CreateNew().Build();

            _releaseWindowRepository.SetupEntities(new[] { releaseWindow });
            _releaseRepository.SetupEntities(Enumerable.Empty<ReleaseRepository>());
            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.UpdateRepository(repo, releaseWindow.ExternalId));

            Assert.IsTrue(ex.Message.Contains(releaseWindow.ExternalId.ToString()));
            Assert.IsTrue(ex.Message.Contains(repo.ExternalId.ToString()));
        }

        [Test]
        public void UpdateRepository_ShouldUpdateRepository_WhenSucced()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew().Build();
            var repo = Builder<Contracts.Plugins.Data.SourceControl.ReleaseRepository>.CreateNew()
                .With(x => x.ChangesFrom, RandomData.RandomString(10))
                .Build();
            var dataRepo = Builder<ReleaseRepository>.CreateNew().Build();

            _releaseWindowRepository.SetupEntities(new[] { releaseWindow });
            _releaseRepository.SetupEntities(new[] { dataRepo });
            _mappingEngine.Setup(x => x.Map(repo, dataRepo)).Returns(dataRepo);
            _releaseRepository.Setup(x => x.Update(dataRepo)).Returns((ChangedFields<ReleaseRepository>)null);

            Sut.UpdateRepository(repo, releaseWindow.ExternalId);

            Assert.AreNotEqual(repo.ChangesFrom, dataRepo.ChangesFrom);
            _releaseRepository.Verify(x => x.Update(It.IsAny<ReleaseRepository>()), Times.Once);
        }

        [Test]
        public void UpdateRepository_ShouldUpdateRepositoryAndNotUpdateChangeFrom_WhenChangeFromEmptyInSourceRepo()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew().Build();
            var repo = Builder<Contracts.Plugins.Data.SourceControl.ReleaseRepository>.CreateNew()
                .With(x => x.ChangesFrom, null)
                .Build();
            var dataRepo = Builder<ReleaseRepository>.CreateNew()
                .With(x => x.ChangesFrom, RandomData.RandomString(10))
                .Build();

            _releaseWindowRepository.SetupEntities(new[] { releaseWindow });
            _releaseRepository.SetupEntities(new[] { dataRepo });
            _mappingEngine.Setup(x => x.Map(repo, dataRepo)).Returns(dataRepo);
            _releaseRepository.Setup(x => x.Update(dataRepo)).Returns((ChangedFields<ReleaseRepository>)null);

            Sut.UpdateRepository(repo, releaseWindow.ExternalId);

            Assert.AreEqual(repo.ChangesFrom, dataRepo.ChangesFrom);
            _releaseRepository.Verify(x => x.Update(It.IsAny<ReleaseRepository>()), Times.Once);
        }

        [Test]
        public void RemoveRepositoriesFromRelease_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            _releaseWindowRepository.SetupEntities(Enumerable.Empty<ReleaseWindow>());
            var releaseWindowId = Guid.NewGuid();

            var ex = Assert.Throws<ReleaseWindowNotFoundException>(() => Sut.RemoveRepositoriesFromRelease(releaseWindowId));

            Assert.IsTrue(ex.Message.Contains(releaseWindowId.ToString()));
        }

        [Test]
        public void RemoveRepositoriesFromRelease_ShouldRemoveFoundRepositories_WhenSucced()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew().Build();
            var dataRepositories = Builder<ReleaseRepository>.CreateListOfSize(5)
                .Random(3)
                .With(x => x.ReleaseWindowId, releaseWindow.ReleaseWindowId)
                .Build();

            _releaseWindowRepository.SetupEntities(new[] { releaseWindow });
            _releaseRepository.SetupEntities(dataRepositories);
            _releaseRepository.Setup(x => x.Delete(It.Is<IEnumerable<ReleaseRepository>>(
                r => r.SequenceEqual(dataRepositories.Where(r2 => r2.ReleaseWindowId == releaseWindow.ReleaseWindowId)))));

            Sut.RemoveRepositoriesFromRelease(releaseWindow.ExternalId);

            _releaseRepository.Verify(x => x.Delete(It.IsAny<IEnumerable<ReleaseRepository>>()), Times.Once());
        }
    }
}
