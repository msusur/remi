using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Products;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data.QaStats;
using ReMi.Contracts.Plugins.Services.QaStats;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryHandlers.ReleasePlan;

namespace ReMi.QueryHandlers.Tests.ReleasePlan
{
    public class GetQaStatusHandlerTests : TestClassFor<GetQaStatusHandler>
    {
        private Mock<IProductGateway> _productGatewayMock;
        private Mock<ICheckQaStatus> _checkQaStatusMock;

        protected override GetQaStatusHandler ConstructSystemUnderTest()
        {
            return new GetQaStatusHandler
            {
                ProductGatewayFactory = () => _productGatewayMock.Object,
                CheckQaStatus = _checkQaStatusMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productGatewayMock = new Mock<IProductGateway>();
            _checkQaStatusMock = new Mock<ICheckQaStatus>();
            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetQaStatusForPackage_WhenPackageUsesQaService()
        {
            var package = Builder<Product>.CreateNew()
                .With(r => r.Description, RandomData.RandomString(5))
                .With(r => r.ExternalId, Guid.NewGuid())
                .Build();

            var qaStatusCheckIten = new QaStatusCheckItem();

            _productGatewayMock.Setup(g => g.GetProduct(package.Description))
                .Returns(package);

            _checkQaStatusMock.Setup(x => x.GetQaStatusCheckItems(new [] { package.ExternalId }))
                .Returns(new List<QaStatusCheckItem>
                {
                    qaStatusCheckIten
                });

            var result = Sut.Handle(new GetQaStatusRequest
            {
                PackageName = package.Description
            });

            Assert.AreEqual(qaStatusCheckIten,result.Content.First());
        }
    }
}
