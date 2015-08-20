using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataAccess.Tests.ReleaseCalendar
{
    public class ReleaseProductGatewayTests : TestClassFor<ReleaseProductGateway>
    {
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IRepository<ReleaseProduct>> _releaseProductRepositoryMock;
        private Mock<IRepository<Product>> _productRepositoryMock;

        protected override ReleaseProductGateway ConstructSystemUnderTest()
        {
            return new ReleaseProductGateway
            {
                ProductRepository = _productRepositoryMock.Object,
                ReleaseProductRepository = _releaseProductRepositoryMock.Object,
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>();
            _productRepositoryMock = new Mock<IRepository<Product>>();
            _releaseProductRepositoryMock = new Mock<IRepository<ReleaseProduct>>();

            base.TestInitialize();
        }

        [Test]
        public void AssignProductsToRelease_ShouldNotInsertNewRows_WhenProductCollectionEmpty()
        {
            var release = SetupReleaseWindow();

            Sut.AssignProductsToRelease(release.ExternalId, new String[0]);

            _releaseProductRepositoryMock.Verify(x => x.Insert(It.IsAny<ReleaseProduct>()), Times.Never);
        }

        [Test]
        public void AssignProductsToRelease_ShouldInsertNewRow_WhenProductNotAssignedToRelease()
        {
            var release = SetupReleaseWindow();

            var products = SetupProducts(new[] { "P1", "P2" });

            SetupReleaseProducts(new[] { "P1" }, release.ExternalId);

            Sut.AssignProductsToRelease(release.ExternalId, new[] { "P1", "P2" });

            _releaseProductRepositoryMock.Verify(x =>
                x.Insert(It.Is<ReleaseProduct>(rp =>
                    rp.ProductId == products.ElementAt(1).ProductId
                    && rp.ReleaseWindowId == release.ReleaseWindowId
                    && rp.CreatedOn != default(DateTime))
                ), Times.Once);
        }

        [Test]
        public void AssignProductsToRelease_ShouldNotInsertNewRow_WhenAllProductsAlreadyAssignedToRelease()
        {
            var release = SetupReleaseWindow();

            var products = SetupProducts(new[] { "P1", "P2" });

            SetupReleaseProducts(new[] { "P1", "P2" }, release.ExternalId);

            Sut.AssignProductsToRelease(release.ExternalId, new[] { "P1", "P2" });

            _releaseProductRepositoryMock.Verify(x =>
                x.Insert(It.IsAny<ReleaseProduct>()), Times.Never);
        }

        private ReleaseWindow SetupReleaseWindow(Guid? releaseWindowId = null)
        {
            var release = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId = releaseWindowId ?? Guid.NewGuid())
                .With(x => x.ReleaseWindowId = RandomData.RandomInt(int.MaxValue))
                .Build();

            _releaseWindowRepositoryMock.SetupEntities(new[] { release });

            return release;
        }

        private IEnumerable<ReleaseProduct> SetupReleaseProducts(IEnumerable<string> productNames = null, Guid? releaseWindowId = null)
        {
            var productsLocal = (productNames ?? Enumerable.Empty<string>()).ToArray();

            var i = 1;
            var j = 1;

            var releaseProducts = productsLocal
                .Select(productLocal =>
                    Builder<ReleaseProduct>.CreateNew()
                        .With(x => x.Product =
                            Builder<Product>.CreateNew()
                                .With(p => p.Description = productLocal)
                                .With(p => p.ProductId = i++)
                                .Build())
                        .With(x => x.ReleaseWindow = Builder<ReleaseWindow>.CreateNew()
                                .With(p => p.ExternalId = releaseWindowId ?? Guid.NewGuid())
                                .With(p => p.ReleaseWindowId = j++)
                                .Build())
                        .Build())
                .ToList();

            _releaseProductRepositoryMock.SetupEntities(releaseProducts);

            return releaseProducts;
        }

        private IEnumerable<Product> SetupProducts(IEnumerable<string> productNames = null)
        {
            var productsLocal = (productNames ?? Enumerable.Empty<string>()).ToArray();

            var i = 1;
            var products = productsLocal
                .Select(productLocal =>
                    Builder<Product>.CreateNew()
                        .With(p => p.Description = productLocal)
                        .With(p => p.ProductId = i++)
                        .Build())
                .ToList();

            _productRepositoryMock.SetupEntities(products);

            return products;

        }
    }
}
