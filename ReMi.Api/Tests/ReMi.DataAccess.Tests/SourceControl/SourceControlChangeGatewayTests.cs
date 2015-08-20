using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.SourceControl;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.DataEntities.Products;
using ContractChanges = ReMi.Contracts.Plugins.Data.SourceControl.SourceControlChange;

namespace ReMi.DataAccess.Tests.SourceControl
{
    public class SourceControlChangeGatewayTests : TestClassFor<SourceControlChangeGateway>
    {
        private Mock<IMappingEngine> _mapperMock;
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IRepository<SourceControlChange>> _changeRepositoryMock;
        private Mock<IRepository<SourceControlChangeToReleaseWindow>> _changeToReleaseWindowRepositoryMock;

        protected override SourceControlChangeGateway ConstructSystemUnderTest()
        {
            return new SourceControlChangeGateway
            {
                Mapper = _mapperMock.Object,
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object,
                SourceControlChangeToReleaseWindowRepository = _changeToReleaseWindowRepositoryMock.Object,
                SourceControlChangeRepository = _changeRepositoryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _mapperMock = new Mock<IMappingEngine>(MockBehavior.Strict);
            _changeRepositoryMock = new Mock<IRepository<SourceControlChange>>(MockBehavior.Strict);
            _changeToReleaseWindowRepositoryMock = new Mock<IRepository<SourceControlChangeToReleaseWindow>>(MockBehavior.Strict);
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Dispose_ShouldCallDisposeForAllRepositories_WhenInvoked()
        {
            _changeRepositoryMock.Setup(o => o.Dispose());
            _changeToReleaseWindowRepositoryMock.Setup(o => o.Dispose());
            _releaseWindowRepositoryMock.Setup(o => o.Dispose());

            Sut.Dispose();

            _changeRepositoryMock.Verify(o => o.Dispose(), Times.Once);
            _changeToReleaseWindowRepositoryMock.Verify(o => o.Dispose(), Times.Once);
            _releaseWindowRepositoryMock.Verify(o => o.Dispose(), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void StoreChanges_ShouldThrowException_WhenReleaseWindowIsNotFound()
        {
            _releaseWindowRepositoryMock.SetupEntities(Enumerable.Empty<ReleaseWindow>());

            Sut.StoreChanges(null, Guid.NewGuid());
        }

        [Test]
        public void StoreChanges_ShouldAddNotExistingChangesAndAssignToRelease_WhenCalled()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseProducts = new List<ReleaseProduct>
                {
                    new ReleaseProduct { Product = new Product { ProductId = RandomData.RandomInt(int.MaxValue) } }
                }
            };
            var changes = Builder<ContractChanges>.CreateListOfSize(10).Build();
            var dataChanges = Builder<SourceControlChange>.CreateListOfSize(10).Build();
            var toInsertChanges = dataChanges.Skip(5).Take(5).ToArray();
            var changesToRelease = dataChanges.Take(5).Select(x => new SourceControlChangeToReleaseWindow
            {
                ReleaseWindow = releaseWindow,
                Change = new SourceControlChange { Identifier = x.Identifier }
            });
            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _changeRepositoryMock.SetupEntities(dataChanges.Take(5));
            _mapperMock.Setup(x => x.Map<IEnumerable<ContractChanges>, IEnumerable<SourceControlChange>>(It.Is<IEnumerable<ContractChanges>>(
                c => changes.Skip(5).Take(5).Intersect(c).Count() == 5)))
                .Returns(toInsertChanges);
            _changeRepositoryMock.Setup(x => x.Insert(toInsertChanges))
                .Callback((IEnumerable<SourceControlChange> x) => _changeRepositoryMock.SetupEntities(dataChanges));
            _changeToReleaseWindowRepositoryMock.SetupEntities(changesToRelease);
            _changeToReleaseWindowRepositoryMock.Setup(x => x.Insert(It.Is<IEnumerable<SourceControlChangeToReleaseWindow>>(
                c => c.Count() == 5)));

            Sut.StoreChanges(changes, releaseWindow.ExternalId);

            _changeRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<SourceControlChange>>()), Times.Once);
            _changeToReleaseWindowRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<SourceControlChangeToReleaseWindow>>()), Times.Once);
        }

        [Test]
        public void RemoveChangesFromRelease_ShouldDoNothing_WhenNoChangesAssignToRelease()
        {
            _changeToReleaseWindowRepositoryMock.SetupEntities(Enumerable.Empty<SourceControlChangeToReleaseWindow>());

            Sut.RemoveChangesFromRelease(Guid.NewGuid());

            _changeToReleaseWindowRepositoryMock.Verify(x => x.Delete(It.IsAny<IEnumerable<SourceControlChangeToReleaseWindow>>()), Times.Never);
        }

        [Test]
        public void RemoveChangesFromRelease_ShouldRemoveExistingChanges_WhenChangesFound()
        {
            var releaseWindow = new ReleaseWindow { ExternalId = Guid.NewGuid() };
            var changes = Builder<SourceControlChangeToReleaseWindow>.CreateListOfSize(5)
                .All()
                .Do(x => x.ReleaseWindow = releaseWindow)
                .Build();
            _changeToReleaseWindowRepositoryMock.SetupEntities(changes);
            _changeToReleaseWindowRepositoryMock.Setup(x => x.Delete(It.Is<IEnumerable<SourceControlChangeToReleaseWindow>>(
                c => c.Intersect(changes).Count() == 5)));

            Sut.RemoveChangesFromRelease(releaseWindow.ExternalId);

            _changeToReleaseWindowRepositoryMock.Verify(x => x.Delete(It.IsAny<IEnumerable<SourceControlChangeToReleaseWindow>>()), Times.Once);
        }

        [Test]
        public void FilterExistingChangesByProduct_ShouldReturnFilteredChangesId_WhenCalled()
        {
            var changes = Enumerable.Range(0, 5).Select(x => Guid.NewGuid().ToString()).ToArray();
            var products = Enumerable.Range(0, 5).Select(x => Guid.NewGuid()).ToArray();
            var dataChanges = changes.Select(x => new SourceControlChangeToReleaseWindow
            {
                ReleaseWindow = new ReleaseWindow { ReleaseProducts = new List<ReleaseProduct>
                {
                    new ReleaseProduct { Product = new Product { ExternalId = RandomData.RandomElement(products) } }
                }},
                Change = new SourceControlChange { Identifier = x }
            }).ToArray();
            _changeToReleaseWindowRepositoryMock.SetupEntities(dataChanges);

            var result = Sut.FilterExistingChangesByProduct(changes, products);

            CollectionAssertHelper.AreEquivalent(changes, result);
        }

        [Test]
        public void GetChanges_ShouldReturnChangesForRelease_WhenCalled()
        {
            var changes = Builder<ContractChanges>.CreateListOfSize(5).Build();
            var dataChanges = changes.Select(x => new SourceControlChangeToReleaseWindow
            {
                ReleaseWindow = new ReleaseWindow { ExternalId = Guid.NewGuid() },
                Change = new SourceControlChange()
            }).ToArray();
            _changeToReleaseWindowRepositoryMock.SetupEntities(dataChanges);
            _mapperMock.Setup(x => x.Map<IEnumerable<SourceControlChange>, IEnumerable<ContractChanges>>(
                It.Is<IEnumerable<SourceControlChange>>(c => c.First() == dataChanges[0].Change && c.Count() == 1)))
                .Returns(changes);

            var result = Sut.GetChanges(dataChanges[0].ReleaseWindow.ExternalId);

            Assert.AreEqual(changes, result);
        }

    }
}
