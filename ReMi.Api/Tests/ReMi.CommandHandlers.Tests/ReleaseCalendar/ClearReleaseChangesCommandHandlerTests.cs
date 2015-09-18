using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;
using ReMi.TestUtils.UnitTests;
using System;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class ClearReleaseChangesCommandHandlerTests : TestClassFor<ClearReleaseChangesCommandHandler>
    {
        private Mock<ISourceControlChangeGateway> _sourceControlChangesGatewayFactory;

        protected override ClearReleaseChangesCommandHandler ConstructSystemUnderTest()
        {
            return new ClearReleaseChangesCommandHandler
            {
                SourceControlChangesGatewayFactory = () => _sourceControlChangesGatewayFactory.Object
            };
        }

        protected override void TestInitialize()
        {
            _sourceControlChangesGatewayFactory = new Mock<ISourceControlChangeGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenInvoked()
        {
            var command = new ClearReleaseChangesCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            _sourceControlChangesGatewayFactory.Setup(x => x.RemoveChangesFromRelease(command.ReleaseWindowId));
            _sourceControlChangesGatewayFactory.Setup(x => x.Dispose());

            Sut.Handle(command);

            _sourceControlChangesGatewayFactory.Verify(o => o.RemoveChangesFromRelease(It.IsAny<Guid>()), Times.Once);
            _sourceControlChangesGatewayFactory.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
