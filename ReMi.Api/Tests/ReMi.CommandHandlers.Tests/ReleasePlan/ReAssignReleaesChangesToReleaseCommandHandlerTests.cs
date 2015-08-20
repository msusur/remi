using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.Commands.SourceControl;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    [TestFixture]
    public class ReAssignReleaesChangesToReleaseCommandHandlerTests : TestClassFor<ReAssignReleaseChangesToReleaseCommandHandler>
    {
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private Mock<ISourceControlChangeGateway> _sourceControlChangeGatewayMock;

        protected override ReAssignReleaseChangesToReleaseCommandHandler ConstructSystemUnderTest()
        {
            return new ReAssignReleaseChangesToReleaseCommandHandler
            {
                CommandDispatcher = _commandDispatcherMock.Object,
                SourceControlChangeGatewayFactory = () => _sourceControlChangeGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _sourceControlChangeGatewayMock = new Mock<ISourceControlChangeGateway>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_()
        {
            var releaseWindowId = Guid.NewGuid();
            var command = new ReAssignReleaseChangesToReleaseCommand
            {
                CommandContext = new CommandContext
                {
                    UserId = Guid.NewGuid()
                },
                ReleaseWindowId = releaseWindowId
            };

            Sut.Handle(command);

            _sourceControlChangeGatewayMock.Verify(x => x.RemoveChangesFromRelease(releaseWindowId));

            _commandDispatcherMock.Verify(x =>
                x.Send(It.Is<StoreSourceControlChangesCommand>(cmd =>
                    cmd.ReleaseWindowId == command.ReleaseWindowId
                    && cmd.CommandContext.UserId == command.CommandContext.UserId
                )));
        }
    }
}
