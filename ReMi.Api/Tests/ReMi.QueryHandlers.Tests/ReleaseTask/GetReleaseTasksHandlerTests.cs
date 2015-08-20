using System;
using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryHandlers.ReleasePlan;

namespace ReMi.QueryHandlers.Tests.ReleaseTask
{
    public class GetReleaseTasksHandlerTests
        : TestClassFor<GetReleaseTasksHandler>
    {
        private Mock<IReleaseTaskGateway> _releaseTaskGatewayMock;

        protected override GetReleaseTasksHandler ConstructSystemUnderTest()
        {
            return new GetReleaseTasksHandler
            {
                ReleaseTaskGatewayFactory = () => _releaseTaskGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseTaskGatewayMock = new Mock<IReleaseTaskGateway>();

            base.TestInitialize();
        }

        [Test]
        public void GetRelaseTasks_ShouldCallGatewayMethodAndDisposeIt_WhenCallCallGateway()
        {
            var releaseWindowId = Guid.NewGuid();
            Sut.Handle(new GetReleaseTasksRequest {ReleaseWindowId = releaseWindowId});

            _releaseTaskGatewayMock.Verify(x => x.GetReleaseTaskViews(releaseWindowId));
            _releaseTaskGatewayMock.Verify(x => x.Dispose());
        }
    }
}
