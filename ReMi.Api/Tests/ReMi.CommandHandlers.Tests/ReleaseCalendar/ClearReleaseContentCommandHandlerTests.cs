using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using System;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class ClearReleaseContentCommandHandlerTests : TestClassFor<ClearReleaseContentCommandHandler>
    {
        private Mock<IReleaseContentGateway> _releaseContentGatewayFactoryMock;

        protected override ClearReleaseContentCommandHandler ConstructSystemUnderTest()
        {
            return new ClearReleaseContentCommandHandler
            {
                ReleaseContentGatewayFactory = () => _releaseContentGatewayFactoryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseContentGatewayFactoryMock = new Mock<IReleaseContentGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenInvoked()
        {
            var command = new ClearReleaseContentCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            _releaseContentGatewayFactoryMock.Setup(x => x.RemoveTicketsFromRelease(command.ReleaseWindowId));
            _releaseContentGatewayFactoryMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseContentGatewayFactoryMock.Verify(o => o.RemoveTicketsFromRelease(It.IsAny<Guid>()), Times.Once);
            _releaseContentGatewayFactoryMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
