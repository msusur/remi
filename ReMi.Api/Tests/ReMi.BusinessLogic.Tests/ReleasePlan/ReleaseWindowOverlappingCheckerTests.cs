using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.Exceptions;
using ReMi.TestUtils.UnitTests;

namespace ReMi.BusinessLogic.Tests.ReleasePlan
{
    public class ReleaseWindowOverlappingCheckerTests : TestClassFor<ReleaseWindowOverlappingChecker>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IProductGateway> _productGatewayMock;
        private Mock<IReleaseWindowHelper> _releaseWindowHelperMock;

        protected override ReleaseWindowOverlappingChecker ConstructSystemUnderTest()
        {
            return new ReleaseWindowOverlappingChecker
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                ProductGatewayFactory = () => _productGatewayMock.Object,
                ReleaseWindowHelper = _releaseWindowHelperMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _productGatewayMock = new Mock<IProductGateway>();

            _releaseWindowHelperMock = new Mock<IReleaseWindowHelper>();
            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(false);

            base.TestInitialize();
        }

        [Test]
        public void FindOverlappedWindow_ShouldReturnNull_WhenReleaseTypeSystemMaintenance()
        {
            var release = SetupReleaseWindow(ReleaseType.SystemMaintenance);

            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(true);

            var result = Sut.FindOverlappedWindow(release);

            Assert.IsNull(result);
        }

        [Test]
        [ExpectedException(typeof(ProductShouldBeAssignedException))]
        public void FindOverlappedWindow_ShouldThrowException_WhenNoProductAssignedToRelease()
        {
            var release = SetupReleaseWindow();

            var result = Sut.FindOverlappedWindow(release);

            Assert.IsNull(result);
        }

        [Test]
        public void FindOverlappedWindow_ShouldReturnNull_WhenProductAutomated()
        {
            var release = SetupReleaseWindow();

            SetupProduct(release.Products, ReleaseTrack.Automated, release.ExternalId);

            var result = Sut.FindOverlappedWindow(release);

            Assert.IsNull(result);
        }

        [Test]
        public void FindOverlappedWindow_ShouldCallGatewayMethod_WhenInvoked()
        {
            var release = SetupReleaseWindow();

            SetupProduct(release.Products, releaseWindowId: release.ExternalId);

            Sut.FindOverlappedWindow(release);

            _releaseWindowGatewayMock.Verify(o => o.FindFirstOverlappedRelease("P1", release.StartTime, release.EndTime, release.ExternalId));
            _releaseWindowGatewayMock.Verify(o => o.FindFirstOverlappedRelease("P2", release.StartTime, release.EndTime, release.ExternalId));
        }

        private ReleaseWindow SetupReleaseWindow(ReleaseType releaseType = ReleaseType.Scheduled)
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ReleaseType, releaseType)
                .With(o => o.Products, new[] { "P1", "P2" })
                .Build();

            _releaseWindowGatewayMock.Setup(o => o.GetByExternalId(releaseWindow.ExternalId, It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(releaseWindow);

            return releaseWindow;
        }

        private IEnumerable<Product> SetupProduct(IEnumerable<string> productNames = null, ReleaseTrack releaseTrack = ReleaseTrack.Manual, Guid? releaseWindowId = null)
        {
            var products = new List<Product>();
            if (productNames == null)
                productNames = new[] { RandomData.RandomString(5) };

            foreach (var productName in productNames)
            {
                var product = Builder<Product>.CreateNew()
                    .With(o => o.Description, productName)
                    .With(o => o.ReleaseTrack, releaseTrack)
                    .Build();

                products.Add(product);

                _productGatewayMock.Setup(o => o.GetProduct(product.Description))
                    .Returns(product);
            }

            if (releaseWindowId.HasValue)
                _productGatewayMock.Setup(o => o.GetProducts(releaseWindowId.Value))
                    .Returns(products);

            if (releaseWindowId.HasValue)
                _productGatewayMock.Setup(o => o.GetProducts(productNames))
                    .Returns(products);

            return products;
        }
    }
}
