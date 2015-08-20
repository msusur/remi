using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class ConfirmReleaseTaskReceiptHandlerTests
        : TestClassFor<ConfirmReleaseTaskReceiptHandler>
    {
        private Mock<IReleaseTaskGateway> _releaseTaskGatewayMock;

        protected override ConfirmReleaseTaskReceiptHandler ConstructSystemUnderTest()
        {
            return new ConfirmReleaseTaskReceiptHandler
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
        public void ConfirmReleaseTaskReceipt_ShouldCallGateway()
        {
            var releaseTaskId = Guid.NewGuid();

            Sut.Handle(new ConfirmReleaseTaskReceiptCommand
            {
                ReleaseTaskId = releaseTaskId
            });

            _releaseTaskGatewayMock.Verify(g => g.ConfirmReleaseTaskReceipt(releaseTaskId));
        }

        [Test]
        public void CleanReleaseTaskReceipt_ShouldCallGateway()
        {
            var releaseTaskId = Guid.NewGuid();

            Sut.Handle(new CleanReleaseTaskReceiptCommand
            {
                ReleaseTaskId = releaseTaskId
            });

            _releaseTaskGatewayMock.Verify(g => g.ClearReleaseTaskReceipt(releaseTaskId));
        }
    }
}
