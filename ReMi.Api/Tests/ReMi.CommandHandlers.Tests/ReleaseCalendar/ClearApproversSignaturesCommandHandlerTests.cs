using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class ClearApproversSignaturesCommandHandlerTests : TestClassFor<ClearApproversSignaturesCommandHandler>
    {
        private Mock<IReleaseApproverGateway> _releaseApproverGatewayMock;

        protected override ClearApproversSignaturesCommandHandler ConstructSystemUnderTest()
        {
            return new ClearApproversSignaturesCommandHandler
            {
                ReleaseApproverGatewayFactory = () => _releaseApproverGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseApproverGatewayMock = new Mock<IReleaseApproverGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenInvoked()
        {
            var command = new ClearApproversSignaturesCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            _releaseApproverGatewayMock.Setup(x => x.ClearApproverSignatures(command.ReleaseWindowId));
            _releaseApproverGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseApproverGatewayMock.Verify(o => o.ClearApproverSignatures(It.IsAny<Guid>()), Times.Once);
            _releaseApproverGatewayMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
