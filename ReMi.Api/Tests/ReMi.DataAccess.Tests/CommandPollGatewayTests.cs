using System;
using Moq;
using FizzWare.NBuilder;
using AutoMapper;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ExecPoll;
using ReMi.DataAccess.BusinessEntityGateways;
using BusinessCommand = ReMi.BusinessEntities.ExecPoll.CommandExecution;
using DataCommand = ReMi.DataEntities.ExecPoll.CommandExecution;
using BusinessCommandStateType = ReMi.BusinessEntities.ExecPoll.CommandStateType;


namespace ReMi.DataAccess.Tests
{
    [TestFixture]
    public class CommandPollGatewayTests : TestClassFor<CommandPollGateway>
    {
        private Mock<IRepository<CommandExecution>> _commandRepositoryMock;
        private Mock<IRepository<CommandHistory>> _commandHistoryRepositoryMock;

        private Mock<IMappingEngine> _mappingEngineMock;

        protected override CommandPollGateway ConstructSystemUnderTest()
        {
            return new CommandPollGateway
            {
                CommandHistoryRepository = _commandHistoryRepositoryMock.Object,
                CommandRepository = _commandRepositoryMock.Object,
                MappingEngine = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandRepositoryMock = new Mock<IRepository<CommandExecution>>();
            _commandHistoryRepositoryMock = new Mock<IRepository<CommandHistory>>();
            _mappingEngineMock = new Mock<IMappingEngine>();

            base.TestInitialize();
        }

        [Test]
        public void GetByExternalId_ShouldReturnNull_WhenCommandIdNotExists()
        {
            var commandId = Guid.NewGuid();

            var result = Sut.GetByExternalId(commandId);

            Assert.IsNull(result);
        }

        [Test]
        public void GetByExternalId_ShouldReturnCommand_WhenInvoked()
        {
            var command = Builder<DataCommand>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _commandRepositoryMock.SetupEntities(new[] { command });

            Sut.GetByExternalId(command.ExternalId);

            _mappingEngineMock.Verify(o => o.Map<DataCommand, BusinessCommand>(
                It.Is<DataCommand>(x =>
                    x.ExternalId == command.ExternalId
                    && x.CreatedOn == command.CreatedOn
                    && x.Description == command.Description
                )));
        }

        [Test]
        public void Create_ShouldInsertToCommandRepository_WhenInvoked()
        {
            var commandId = Guid.NewGuid();
            var description = RandomData.RandomString(100);

            Sut.Create(commandId, description);

            _commandRepositoryMock.Verify(o => o.Insert(It.Is<DataCommand>(row => row.ExternalId == commandId && row.Description == description)));
        }

        [Test]
        public void Create_ShouldInsertToCommandHistoryRepository_WhenInvoked()
        {
            var commandId = Guid.NewGuid();
            var description = RandomData.RandomString(100);

            _commandRepositoryMock.Setup(o => o.Insert(It.IsAny<DataCommand>()))
                .Callback<DataCommand>(command => command.CommandExecutionId = RandomData.RandomInt(1, 100));

            Sut.Create(commandId, description);

            _commandHistoryRepositoryMock.Verify(o => o.Insert(It.Is<CommandHistory>(row => row.State == CommandStateType.Waiting && row.CommandExecutionId > 0)));
        }

        [Test]
        public void SetState_ShouldInsertToCommandHistoryRepository_WhenInvoked()
        {
            var command = Builder<DataCommand>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();
            var businessState = RandomData.RandomEnum<BusinessCommandStateType>();
            var dataState = RandomData.RandomEnum<CommandStateType>();

            _mappingEngineMock.Setup(o => o.Map<BusinessCommandStateType, CommandStateType>(It.IsAny<BusinessCommandStateType>()))
                .Returns(dataState);

            _commandRepositoryMock.SetupEntities(new[] { command });

            Sut.SetState(command.ExternalId, businessState, null);

            _commandHistoryRepositoryMock.Verify(o => o.Insert(It.Is<CommandHistory>(row => row.State == dataState && row.CommandExecutionId == command.CommandExecutionId)));
        }

        [Test]
        [ExpectedException(typeof(CommandExecutionNotFoundException))]
        public void SetState_ShouldRaiseException_WhenCommandNotExists()
        {
            var businessState = RandomData.RandomEnum<BusinessCommandStateType>();
            var dataState = RandomData.RandomEnum<CommandStateType>();

            _mappingEngineMock.Setup(o => o.Map<BusinessCommandStateType, CommandStateType>(It.IsAny<BusinessCommandStateType>()))
                .Returns(dataState);

            Sut.SetState(Guid.NewGuid(), businessState, null);
        }
    }
}
