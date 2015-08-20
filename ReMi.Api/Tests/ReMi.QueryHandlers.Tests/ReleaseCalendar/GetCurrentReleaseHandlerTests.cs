using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Queries.ReleaseCalendar;
using ReMi.QueryHandlers.ReleaseCalendar;

namespace ReMi.QueryHandlers.Tests.ReleaseCalendar
{
    public class GetCurrentReleaseHandlerTests : TestClassFor<GetCurrentReleaseHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;

        protected override GetCurrentReleaseHandler ConstructSystemUnderTest()
        {
            return new GetCurrentReleaseHandler
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetCurrentRelease_WhenInvoked()
        {
            var request = Builder<GetCurrentReleaseRequest>.CreateNew().Build();
            var release = new ReleaseWindow();

            _releaseWindowGatewayMock.Setup(x => x.GetCurrentRelease(request.Product))
                .Returns(release);
            _releaseWindowGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            Assert.AreEqual(release, result.ReleaseWindow);
            _releaseWindowGatewayMock.Verify(o => o.GetCurrentRelease(It.IsAny<string>()), Times.Once);
        }
    }
}
