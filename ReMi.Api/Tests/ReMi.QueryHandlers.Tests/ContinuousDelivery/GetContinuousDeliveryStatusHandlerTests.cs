using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.ReleaseContent;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.QueryHandlers.ContinuousDelivery;

namespace ReMi.QueryHandlers.Tests.ContinuousDelivery
{
    [Ignore("Ignore should be removed in November when feature turns on")]
    public class GetContinuousDeliveryStatusHandlerTests : TestClassFor<GetContinuousDeliveryStatusHandler>
    {
        private Mock<IReleaseContent> _releaseContentMock;
        private Mock<IProductGateway> _packageGatewayMock;

        protected override GetContinuousDeliveryStatusHandler ConstructSystemUnderTest()
        {
            return new GetContinuousDeliveryStatusHandler
            {
                ReleaseContent = _releaseContentMock.Object,
                PackageGatewayFactory = () => _packageGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseContentMock = new Mock<IReleaseContent>();
            _packageGatewayMock = new Mock<IProductGateway>();

            base.TestInitialize();
        }
    }
}
