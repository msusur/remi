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
    public class UpdateAccountCommandHandlerTests : TestClassFor<UpdateAccountCommandHandler>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;

        protected override UpdateAccountCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateAccountCommandHandler
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
            var command = new UpdateAccountCommand
            {
                Account = Builder<Account>.CreateNew().Build()
            };
            _accountsGatewayMock.Setup(x => x.UpdateAccount(command.Account));
            _accountsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _accountsGatewayMock.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
