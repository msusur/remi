using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.CommandHandlers.Auth;
using ReMi.Commands.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.TestUtils.UnitTests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ReMi.CommandHandlers.Tests.Auth
{
    public class CheckAccountsCommandHandlerTests : TestClassFor<CheckAccountsCommandHandler>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;

        protected override CheckAccountsCommandHandler ConstructSystemUnderTest()
        {
            return new CheckAccountsCommandHandler
            {
                AccountsGatewayFactory = () => _accountsGatewayMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountsGatewayMock = new Mock<IAccountsGateway>(MockBehavior.Strict);
            _commandDispatcherMock = new Mock<ICommandDispatcher>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToCreateNotExistingAccountAndNotSendCommand_WhenAllAccountsExist()
        {
            var command = new CheckAccountsCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                Accounts = Builder<Account>.CreateListOfSize(5).Build()
            };
            _accountsGatewayMock.Setup(x => x.CreateNotExistingAccount(It.Is<Account>(a => command.Accounts.Contains(a))))
                .Returns(false);
            _accountsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _accountsGatewayMock.Verify(x => x.CreateNotExistingAccount(It.IsAny<Account>()), Times.Exactly(5));
            _accountsGatewayMock.Verify(x => x.Dispose(), Times.Once);
            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ICommand>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldCallGatewayToCreateNotExistingAccountAndSendCommand_WhenAtLeastOneAccountWasCreated()
        {
            var command = new CheckAccountsCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                Accounts = Builder<Account>.CreateListOfSize(5).Build(),
                CommandContext = new CommandContext { Id = Guid.NewGuid() }
            };
            _accountsGatewayMock.Setup(x => x.CreateNotExistingAccount(It.Is<Account>(a => command.Accounts.Contains(a))))
                .Returns(true);
            _accountsGatewayMock.Setup(x => x.Dispose());
            _commandDispatcherMock.Setup(x => x.Send(It.Is<AssociateAccountsWithProductCommand>(c =>
                c.Accounts.SequenceEqual(command.Accounts)
                && c.ReleaseWindowId == command.ReleaseWindowId
                && c.CommandContext.ParentId == command.CommandContext.Id)))
                .Returns((Task)null);

            Sut.Handle(command);

            _accountsGatewayMock.Verify(x => x.CreateNotExistingAccount(It.IsAny<Account>()), Times.Exactly(5));
            _accountsGatewayMock.Verify(x => x.Dispose(), Times.Once);
            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ICommand>()), Times.Once);
        }
    }
}
