using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryHandlers.ReleasePlan;

namespace ReMi.QueryHandlers.Tests.ReleaseTask
{
    public class GetReleaseTaskTypesHandlerTests : TestClassFor<GetReleaseTaskTypesHandler>
    {
        private Mock<IReleaseTaskGateway> _releaseTaskGatewayMock;

        protected override GetReleaseTaskTypesHandler ConstructSystemUnderTest()
        {
            return new GetReleaseTaskTypesHandler {ReleaseTaskGatewayFactory = () => _releaseTaskGatewayMock.Object};
        }

        protected override void TestInitialize()
        {
            _releaseTaskGatewayMock = new Mock<IReleaseTaskGateway>();

            base.TestInitialize();
        }

        [Test]
        public void GetReleaseTaskTypes_ShouldCallGatewayMethodAndDisposeIt_WhenCallCallGateway()
        {
            Sut.Handle(new GetReleaseTaskTypesRequest());

            _releaseTaskGatewayMock.Verify(x => x.GetReleaseTaskTypes());
            _releaseTaskGatewayMock.Verify(x => x.Dispose());
        }
    }
}
