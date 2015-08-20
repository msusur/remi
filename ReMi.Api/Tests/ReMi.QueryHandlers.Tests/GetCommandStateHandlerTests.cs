using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ExecPoll;
using ReMi.BusinessLogic.ExecPoll;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.ExecPoll;
using ReMi.QueryHandlers.ExecPoll;
using BusinessCommandStateType = ReMi.BusinessEntities.ExecPoll.CommandStateType;

namespace ReMi.QueryHandlers.Tests
{
    [TestFixture]
    public class GetCommandStateHandlerTests : TestClassFor<GetCommandStateHandler>
    {
        private Mock<ICommandPollBusinessLogic> _commandPollBusinessLogicMock;

        protected override GetCommandStateHandler ConstructSystemUnderTest()
        {
            return new GetCommandStateHandler
            {
                CommandPollBusinessLogic = _commandPollBusinessLogicMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandPollBusinessLogicMock = new Mock<ICommandPollBusinessLogic>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetCommandState_WhenInvoked()
        {
            var request = Builder<GetCommandStateRequest>.CreateNew().Build();

            _commandPollBusinessLogicMock.Setup(o => o.GetCommandExecutionState(request.ExternalId))
                .Returns(new CommandState { StateType = CommandStateType.Success });

            Sut.Handle(request);

            _commandPollBusinessLogicMock.Verify(o => o.GetCommandExecutionState(It.Is<Guid>(cmd => cmd == request.ExternalId)));
        }

        [Test]
        public void Handle_ShouldGetCommandState_WhenInvoked2()
        {
            var state = RandomData.RandomEnum<BusinessCommandStateType>();

            var request = Builder<GetCommandStateRequest>.CreateNew().Build();

            _commandPollBusinessLogicMock.Setup(o => o.GetCommandExecutionState(request.ExternalId))
                .Returns(new CommandState { StateType = state, Details = "test" });

            var response = Sut.Handle(request);

            Assert.IsNotNull(response);
            Assert.AreEqual(state, response.State);
            Assert.AreEqual("test", response.Details);
        }

    }
}
