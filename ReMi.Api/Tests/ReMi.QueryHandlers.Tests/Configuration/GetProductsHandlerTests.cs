using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Products;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Queries.Configuration;
using ReMi.QueryHandlers.Configuration;

namespace ReMi.QueryHandlers.Tests.Configuration
{
    public class GetProductsHandlerTests : TestClassFor<GetProductsHandler>
    {
        private Mock<IProductGateway> _productGatewayMock;

        protected override GetProductsHandler ConstructSystemUnderTest()
        {
            return new GetProductsHandler
            {
                ProductGatewayFactory = () => _productGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productGatewayMock = new Mock<IProductGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldReturnProductList()
        {
            var guid = Guid.NewGuid();
            _productGatewayMock.Setup(p => p.GetAllProducts())
                .Returns(new List<ProductView> {new ProductView {Name = "qwerty", ExternalId = guid}});

            var result = Sut.Handle(new GetProductsRequest());

            Assert.AreEqual(1, result.Products.Count, "product list length");
            Assert.AreEqual("qwerty", result.Products[0].Name, "product name");
            Assert.AreEqual(guid, result.Products[0].ExternalId, "product external id");
        }
    }
}
