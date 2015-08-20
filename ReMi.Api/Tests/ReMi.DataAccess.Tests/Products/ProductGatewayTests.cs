using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Exceptions.Configuration;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using BusinessProduct = ReMi.BusinessEntities.Products.Product;
using BusinessProductView = ReMi.BusinessEntities.Products.ProductView;
using DataProduct = ReMi.DataEntities.Products.Product;

namespace ReMi.DataAccess.Tests.Products
{
    [TestFixture]
    public class ProductGatewayTests : TestClassFor<ProductGateway>
    {
        private Mock<IRepository<DataProduct>> _productRepositoryMock;
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IRepository<BusinessUnit>> _businessUnitRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override ProductGateway ConstructSystemUnderTest()
        {
            _productRepositoryMock = new Mock<IRepository<DataProduct>>(MockBehavior.Strict);
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>(MockBehavior.Strict);
            _businessUnitRepositoryMock = new Mock<IRepository<BusinessUnit>>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);

            return new ProductGateway
            {
                ProductRepository = _productRepositoryMock.Object,
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object,
                BusinessUnitRepository = _businessUnitRepositoryMock.Object,
                MappingEngine = _mappingEngineMock.Object
            };
        }

        [Test]
        public void Dispose_ShouldDisposedAllRepositories_WhenDisposed()
        {
            _productRepositoryMock.Setup(x => x.Dispose());
            _releaseWindowRepositoryMock.Setup(x => x.Dispose());
            _businessUnitRepositoryMock.Setup(x => x.Dispose());

            Sut.Dispose();

            _productRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _releaseWindowRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _businessUnitRepositoryMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void GetProduct_ShouldReturnPropertMappedBusinessProductEntity_WhenReleaseWindowIdMatched()
        {
            var releaseWindowId = Guid.NewGuid();
            var releaseProducts = new[]
            {
                new ReleaseProduct { Product = Builder<DataProduct>.CreateNew().Build(), CreatedOn = SystemTime.Now},
                new ReleaseProduct { Product = Builder<DataProduct>.CreateNew().Build(), CreatedOn = SystemTime.Now}
            };
            var releaseWindow = new ReleaseWindow { ExternalId = releaseWindowId, ReleaseProducts = releaseProducts };
            var requiredProducts = releaseProducts.Select(o => o.Product).ToList();
            var expected = new[] { new BusinessProduct() };

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _mappingEngineMock.Setup(o => o.Map<IEnumerable<DataProduct>, IEnumerable<BusinessProduct>>(requiredProducts))
                .Returns(expected);

            var actual = Sut.GetProducts(releaseWindowId);

            Assert.AreEqual(expected, actual);
            _mappingEngineMock.Verify(o => o.Map<IEnumerable<DataProduct>, IEnumerable<BusinessProduct>>(It.IsAny<IEnumerable<DataProduct>>()), Times.Once);
        }

        [Test]
        public void GetProduct_ShouldReturnPropertMappedBusinessProductEntity_WhenNameMatched()
        {
            var productName = RandomData.RandomString(10);
            var product = new DataProduct { Description = productName };
            var expected = new BusinessProduct();

            _productRepositoryMock.SetupEntities(new[] { product });
            _mappingEngineMock.Setup(o => o.Map<DataProduct, BusinessProduct>(product))
                .Returns(expected);

            var actual = Sut.GetProduct(productName);

            Assert.AreEqual(expected, actual);
            _mappingEngineMock.Verify(o => o.Map<DataProduct, BusinessProduct>(It.IsAny<DataProduct>()), Times.Once);
        }

        [Test]
        public void GetProduct_ShouldThrowException_WhenPackageNotFound()
        {
            _productRepositoryMock.SetupEntities(Enumerable.Empty<DataProduct>());
            var name = RandomData.RandomString(10);

            var ex = Assert.Throws<ProductNotFoundException>(() => Sut.GetProduct(name));

            Assert.IsTrue(ex.Message.Contains(name));
        }

        [Test]
        public void GetAllProducts_ShouldReturnPropertMappedBusinessProductEntity_WhenInvoked()
        {
            var productEnitites = new[] {
                new DataProduct(),
                new DataProduct()
            };
            var expected = Enumerable.Empty<BusinessProductView>();
            _productRepositoryMock.SetupEntities(productEnitites);
            _mappingEngineMock.Setup(o => o.Map<IEnumerable<DataProduct>, IEnumerable<BusinessProductView>>(productEnitites))
                .Returns(expected);

            var actual = Sut.GetAllProducts();

            Assert.AreEqual(expected, actual);
            _mappingEngineMock.Verify(o => o.Map<IEnumerable<DataProduct>, IEnumerable<BusinessProductView>>(It.IsAny<IEnumerable<DataProduct>>()), Times.Once);
        }

        [Test]
        public void AddProduct_ShouldThrowException_WhenBusinessUnitNotExists()
        {
            _businessUnitRepositoryMock.SetupEntities(Enumerable.Empty<BusinessUnit>());

            var request = new BusinessProduct
            {
                BusinessUnit = new BusinessEntities.Products.BusinessUnit { ExternalId = Guid.NewGuid() }
            };

            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.AddProduct(request));

            Assert.IsTrue(ex.Message.Contains(request.BusinessUnit.ExternalId.ToString()));
        }
        [Test]
        [ExpectedException(typeof(ProductAlreadyExistsException))]
        public void AddProduct_ShouldThrowException_WhenProductNotExists()
        {
            var businessUnit = new BusinessUnit
            {
                ExternalId = Guid.NewGuid()
            };
            var productEnitites = new[] {
                new DataProduct{Description = RandomData.RandomString(5), ExternalId = Guid.NewGuid()},
                new DataProduct{Description = RandomData.RandomString(5), ExternalId = Guid.NewGuid()}
            };
            _productRepositoryMock.SetupEntities(productEnitites);
            _businessUnitRepositoryMock.SetupEntities(new[] { businessUnit });

            Sut.AddProduct(new BusinessProduct
            {
                ExternalId = productEnitites[1].ExternalId,
                BusinessUnit = new BusinessEntities.Products.BusinessUnit { ExternalId = businessUnit.ExternalId }
            });
        }

        [Test]
        public void AddProduct_ShouldAddNewProduct_WhenInvoked()
        {
            var businessUnit = new BusinessUnit
            {
                BusinessUnitId = RandomData.RandomInt(100),
                Description = RandomData.RandomString(10),
                ExternalId = Guid.NewGuid(),
                Name = RandomData.RandomString(10)
            };
            var productEnitites = new[] {
                new DataProduct
                {
                    Description = RandomData.RandomString(5),
                    ExternalId = Guid.NewGuid(),
                    BusinessUnit = businessUnit
                },
                new DataProduct{Description = RandomData.RandomString(5)}
            };
            _productRepositoryMock.SetupEntities(productEnitites);
            _businessUnitRepositoryMock.SetupEntities(new[] { businessUnit });
            var product = new BusinessProduct
            {
                Description = RandomData.RandomString(6, 7),
                ExternalId = Guid.NewGuid(),
                ChooseTicketsByDefault = RandomData.RandomBool(),
                BusinessUnit = new BusinessEntities.Products.BusinessUnit
                {
                    ExternalId = businessUnit.ExternalId
                },
            };
            _productRepositoryMock.Setup(
                p =>
                    p.Insert(
                        It.Is<DataProduct>(
                            d => d.Description == product.Description
                                && d.ExternalId == product.ExternalId
                                && d.ChooseTicketsByDefault == product.ChooseTicketsByDefault
                                && d.BusinessUnitId == businessUnit.BusinessUnitId)));


            Sut.AddProduct(product);

            _productRepositoryMock.Verify(p => p.Insert(It.IsAny<DataProduct>()), Times.Once);
        }

        [Test]
        public void UpdateProduct_ShouldThrowException_WhenBusinessUnitNotExists()
        {
            _businessUnitRepositoryMock.SetupEntities(Enumerable.Empty<BusinessUnit>());

            var request = new BusinessProduct
            {
                BusinessUnit = new BusinessEntities.Products.BusinessUnit { ExternalId = Guid.NewGuid() }
            };

            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.UpdateProduct(request));

            Assert.IsTrue(ex.Message.Contains(request.BusinessUnit.ExternalId.ToString()));
        }
        [Test]
        [ExpectedException(typeof(ProductNotFoundException))]
        public void UpdateProduct_ShouldThrowException_WhenProductDoesNotExist()
        {
            var businessUnit = new BusinessUnit
            {
                ExternalId = Guid.NewGuid()
            };
            var productEnitites = new[] {
                new DataProduct{Description = RandomData.RandomString(5), ExternalId = Guid.NewGuid()},
                new DataProduct{Description = RandomData.RandomString(5), ExternalId = Guid.NewGuid()}
            };
            _productRepositoryMock.SetupEntities(productEnitites);
            _businessUnitRepositoryMock.SetupEntities(new[] { businessUnit });

            Sut.UpdateProduct(new BusinessProduct
            {
                ExternalId = Guid.NewGuid(),
                BusinessUnit = new BusinessEntities.Products.BusinessUnit { ExternalId = businessUnit.ExternalId }
            });
        }

        [Test]
        public void UpdateProduct_ShouldUpdateProduct_WhenInvoked()
        {
            var businessUnit = new BusinessUnit
            {
                BusinessUnitId = RandomData.RandomInt(100),
                Description = RandomData.RandomString(10),
                ExternalId = Guid.NewGuid(),
                Name = RandomData.RandomString(10)
            };
            var productEnitites = new[] {
                new DataProduct{
                    Description = RandomData.RandomString(5),
                    ExternalId = Guid.NewGuid()
                },
                new DataProduct{Description = RandomData.RandomString(5)}
            };
            _businessUnitRepositoryMock.SetupEntities(new[] { businessUnit });
            _productRepositoryMock.SetupEntities(productEnitites);
            var product = new BusinessProduct
            {
                Description = RandomData.RandomString(6, 7),
                ExternalId = productEnitites[0].ExternalId,
                ChooseTicketsByDefault = true,
                BusinessUnit = new BusinessEntities.Products.BusinessUnit
                {
                    ExternalId = businessUnit.ExternalId
                },
            };

            _productRepositoryMock.Setup(
                p =>
                    p.Update(
                        It.Is<DataProduct>(
                            d =>
                                d.Description == product.Description
                                && d.ExternalId == product.ExternalId
                                && d.ChooseTicketsByDefault == product.ChooseTicketsByDefault
                                && d.BusinessUnitId == businessUnit.BusinessUnitId)))
                                .Returns((ChangedFields<Product>) null);

            Sut.UpdateProduct(product);

            _productRepositoryMock.Verify(p =>p.Update(It.IsAny<DataProduct>()), Times.Once);
        }
    }
}
