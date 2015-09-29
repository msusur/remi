using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.CommandHandlers.Auth;
using ReMi.Commands.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandHandlers.Tests.Auth
{
    public class CreateAccountCommandHandlerTests : TestClassFor<CreateAccountCommandHandler>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;

        protected override CreateAccountCommandHandler ConstructSystemUnderTest()
        {
            return new CreateAccountCommandHandler
            {
                AccountsGatewayFactory = () => _accountsGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountsGatewayMock = new Mock<IAccountsGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToCreateAccount_WhenCalled()
        {
            var command = new CreateAccountCommand
            {
                Account = Builder<Account>.CreateNew().Build()
            };
            _accountsGatewayMock.Setup(x => x.CreateAccount(command.Account, true)).Returns((Account)null);
            _accountsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _accountsGatewayMock.Verify(x => x.CreateAccount(It.IsAny<Account>(), It.IsAny<bool>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
