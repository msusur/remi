using System;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ExecPoll;
using ReMi.BusinessLogic.ExecPoll;
using ReMi.CommandHandlers.ExecPoll;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandHandlers.Tests
{
    [TestFixture]
    public class TrackCommandHandlerTests : TestClassFor<CommandTrackerHandler>
    {
        private Mock<ICommandPollBusinessLogic> _commandPollBusinessLogicMock;

        protected override CommandTrackerHandler ConstructSystemUnderTest()
        {
            return new CommandTrackerHandler(_commandPollBusinessLogicMock.Object);
        }

        protected override void TestInitialize()
        {
            _commandPollBusinessLogicMock = new Mock<ICommandPollBusinessLogic>();

            base.TestInitialize();
        }

        [Test]
        public void CreateCommand_ShouldCallStartCommandState_WhenInvoked()
        {
            var commandId = Guid.NewGuid();
            var description = RandomData.RandomString(RandomData.RandomInt(1, 100));

            Sut.CreateTracker(commandId, description);

            _commandPollBusinessLogicMock.Verify(o => o.StartCommandExecution(It.Is<Guid>(x => x == commandId), It.Is<string>(x => x == description)));
        }

        [Test]
        public void StartCommand_ShouldCallSetCommandState_WhenInvoked()
        {
            var commandId = Guid.NewGuid();

            Sut.Started(commandId);

            _commandPollBusinessLogicMock.Verify(o => o.SetCommandExecutionState(It.Is<Guid>(x => x == commandId), It.Is<CommandStateType>(x => x == CommandStateType.Running), null));
        }

        [Test]
        public void EndCommand_ShouldSetFailedState_WhenInvokedWithFailure()
        {
            var commandId = Guid.NewGuid();

            Sut.Finished(commandId, "error");

            _commandPollBusinessLogicMock.Verify(o => o.SetCommandExecutionState(It.Is<Guid>(x => x == commandId), It.Is<CommandStateType>(x => x == CommandStateType.Failed), "error"));
        }

        [Test]
        public void EndCommand_ShouldSetFailedState_WhenInvokedWithSuccess()
        {
            var commandId = Guid.NewGuid();

            Sut.Finished(commandId);

            _commandPollBusinessLogicMock.Verify(o => o.SetCommandExecutionState(It.Is<Guid>(x => x == commandId), It.Is<CommandStateType>(x => x == CommandStateType.Success), null));
        }

    }
}
