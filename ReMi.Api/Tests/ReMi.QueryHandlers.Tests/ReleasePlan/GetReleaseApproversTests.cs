using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryHandlers.ReleasePlan;

namespace ReMi.QueryHandlers.Tests.ReleasePlan
{
    public class GetReleaseApproversTests : TestClassFor<GetReleaseApproversQueryHandler>
    {
        private Mock<IReleaseApproverGateway> _releaseApproverGatewayMock;

        protected override GetReleaseApproversQueryHandler ConstructSystemUnderTest()
        {
            return new GetReleaseApproversQueryHandler
            {
                ReleaseApproverGateway = () => _releaseApproverGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseApproverGatewayMock = new Mock<IReleaseApproverGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayAndReturnCorrectValue()
        {
            var request = new GetReleaseApproversRequest {ReleaseWindowId = Guid.NewGuid()};
            var approvers = new List<ReleaseApprover> {new ReleaseApprover()};
            _releaseApproverGatewayMock.Setup(r => r.GetApprovers(request.ReleaseWindowId)).Returns(approvers);

            var result = Sut.Handle(request);

            Assert.AreEqual(approvers, result.Approvers, "approvers are not correct");
            _releaseApproverGatewayMock.Verify(r => r.GetApprovers(request.ReleaseWindowId));
        }
    }
}
