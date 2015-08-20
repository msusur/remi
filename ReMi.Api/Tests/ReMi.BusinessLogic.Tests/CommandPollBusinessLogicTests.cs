using System;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ExecPoll;
using ReMi.BusinessLogic.ExecPoll;
using ReMi.Common.Utils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways;
using ReMi.DataAccess.Exceptions;

namespace ReMi.BusinessLogic.Tests
{
    [TestFixture]
    public class CommandPollBusinessLogicTests : TestClassFor<CommandPollBusinessLogic>
    {
        private Mock<ICommandPollGateway> _commandPollGatewayMock;


        protected override void TestInitialize()
        {
            _commandPollGatewayMock = new Mock<ICommandPollGateway>();

            base.TestInitialize();
        }

        protected override CommandPollBusinessLogic ConstructSystemUnderTest()
        {
            return new CommandPollBusinessLogic { CommandPollGatewayFactory = () => _commandPollGatewayMock.Object };
        }

        [Test]
        public void GetCommandState_ShouldReturnNotRegistered_WhenInvokedWithNonExistingCommandId()
        {
            var commandId = new Guid();

            var state = Sut.GetCommandExecutionState(commandId);

            Assert.AreEqual(CommandStateType.NotRegistered, state.StateType);
        }

        [Test]
        public void GetCommandState_ShouldReturnExpectedState_WhenInvoked()
        {
            var actualState = RandomData.RandomEnum<CommandStateType>();

            var commandId = new Guid();

            _commandPollGatewayMock.Setup(o => o.GetByExternalId(commandId))
                .Returns(new CommandExecution { State = actualState, Details = "test" });

            var state = Sut.GetCommandExecutionState(commandId);

            Console.WriteLine("Actual state: {0}, check state: {1}", actualState, state);
            Assert.AreEqual(actualState, state.StateType);
            Assert.AreEqual("test", state.Details);
        }

        [Test]
        [ExpectedException(typeof(CommandExecutionNotFoundException))]
        public void SetCommandState_ShouldRaiseException_WhenInvokedForNonExistingCommand()
        {
            var state = RandomData.RandomEnum<CommandStateType>();
            var commandId = new Guid();

            Sut.SetCommandExecutionState(commandId, state, null);
        }

        [Test]
        public void SetCommandState_ShouldCallGatewayMethod_WhenInvoked()
        {
            var state = RandomData.RandomEnum<CommandStateType>();
            var commandId = new Guid();

            _commandPollGatewayMock.Setup(o => o.GetByExternalId(commandId))
                .Returns(new CommandExecution { ExternalId = commandId });

            Sut.SetCommandExecutionState(commandId, state, "error");

            _commandPollGatewayMock.Verify(o => o.SetState(commandId, state, "error"));
        }


        [Test]
        [ExpectedException(typeof(CommandDuplicationException))]
        public void StartCommandState_ShouldRaiseException_WhenInvokedWithExistingCommandId()
        {
            var commandId = new Guid();
            var description = RandomData.RandomString(100);

            _commandPollGatewayMock.Setup(o => o.GetByExternalId(commandId))
                .Returns(new CommandExecution { ExternalId = commandId });

            Sut.StartCommandExecution(commandId, description);
        }

        [Test]
        public void StartCommandState_ShouldCallGatewayMethod_WhenInvoked()
        {
            var commandId = new Guid();
            var description = RandomData.RandomString(100);

            Sut.StartCommandExecution(commandId, description);

            _commandPollGatewayMock.Verify(o => o.Create(commandId, description));
        }

    }
}
