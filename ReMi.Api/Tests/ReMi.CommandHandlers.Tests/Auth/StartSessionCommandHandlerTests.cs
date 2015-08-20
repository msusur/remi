using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.CommandHandlers.Auth;
using ReMi.Commands.Auth;
using ReMi.Common.Constants.BusinessRules;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data.Authentication;
using ReMi.Contracts.Plugins.Services.Authentication;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;

namespace ReMi.CommandHandlers.Tests.Auth
{
    public class StartSessionCommandHandlerTests : TestClassFor<StartSessionCommandHandler>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<IAuthenticationService> _authenticationService;
        private Mock<IBusinessRuleEngine> _businessRuleEngineMock;

        protected override StartSessionCommandHandler ConstructSystemUnderTest()
        {
            return new StartSessionCommandHandler
            {
                AuthenticationService = _authenticationService.Object,
                AccountsGatewayFactory = () => _accountsGatewayMock.Object,
                BusinessRuleEngine = _businessRuleEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _authenticationService = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _accountsGatewayMock = new Mock<IAccountsGateway>(MockBehavior.Strict);
            _businessRuleEngineMock = new Mock<IBusinessRuleEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldThrowException_WhenFailedToAuthenticateInService()
        {
            var command = Builder<StartSessionCommand>.CreateNew()
                .Build();
            _authenticationService.Setup(x => x.GetAccount(command.Login, command.Password))
                .Returns((Account)null);

            var ex = Assert.Throws<FailedToAuthenticateException>(() => Sut.Handle(command));

            Assert.IsTrue(ex.Message.Contains(command.Login));
        }

        [Test]
        public void Handle_ShouldThrowException_WhenAccountIsBlocked()
        {
            var command = Builder<StartSessionCommand>.CreateNew().Build();
            var serviceAccount = Builder<Account>.CreateNew().Build();
            var account = Builder<BusinessAccount>.CreateNew()
                .With(x => x.IsBlocked, true)
                .Build();

            _authenticationService.Setup(x => x.GetAccount(command.Login, command.Password)).Returns(serviceAccount);
            _accountsGatewayMock.Setup(x => x.GetAccountByEmail(serviceAccount.Mail)).Returns(account);
            _accountsGatewayMock.Setup(x => x.Dispose());

            var ex = Assert.Throws<FailedToAuthenticateException>(() => Sut.Handle(command));

            Assert.IsTrue(ex.Message.Contains(command.Login));
            _authenticationService.Verify(x => x.GetAccount(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.GetAccountByEmail(It.IsAny<string>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.CreateAccount(It.IsAny<BusinessAccount>(), It.IsAny<bool>()), Times.Never);
            _accountsGatewayMock.Verify(x => x.StartSession(It.IsAny<BusinessAccount>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
            _businessRuleEngineMock.Verify(x => x.Execute<int>(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, object>>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldStartSession_WhenUserAuthenticatedWithExistingAccount()
        {
            var command = Builder<StartSessionCommand>.CreateNew().Build();
            var serviceAccount = Builder<Account>.CreateNew().Build();
            var account = Builder<BusinessAccount>.CreateNew().Build();
            var session = Builder<BusinessEntities.Auth.Session>.CreateNew().Build();

            _authenticationService.Setup(x => x.GetAccount(command.Login, command.Password)).Returns(serviceAccount);
            _accountsGatewayMock.Setup(x => x.GetAccountByEmail(serviceAccount.Mail)).Returns(account);
            _accountsGatewayMock.Setup(x => x.StartSession(account, command.SessionId, 15)).Returns(session);
            _businessRuleEngineMock.Setup(x => x.Execute<int>(Guid.Empty, BusinessRuleConstants.Config.SessionDurationRule.ExternalId, null)).Returns(15);
            _accountsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _authenticationService.Verify(x => x.GetAccount(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.GetAccountByEmail(It.IsAny<string>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.CreateAccount(It.IsAny<BusinessAccount>(), It.IsAny<bool>()), Times.Never);
            _accountsGatewayMock.Verify(x => x.StartSession(It.IsAny<BusinessAccount>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once);
            _businessRuleEngineMock.Verify(x => x.Execute<int>(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, object>>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldCreateBasicUserAccountAndStartSession_WhenUserAuthenticatedWithNotExistingAccountAndAdminExists()
        {
            var command = Builder<StartSessionCommand>.CreateNew().Build();
            var serviceAccount = Builder<Account>.CreateNew().Build();
            var account = Builder<BusinessAccount>.CreateNew().Build();
            var session = Builder<BusinessEntities.Auth.Session>.CreateNew().Build();

            _authenticationService.Setup(x => x.GetAccount(command.Login, command.Password)).Returns(serviceAccount);
            _accountsGatewayMock.Setup(x => x.GetAccountByEmail(serviceAccount.Mail)).Returns((BusinessAccount)null);
            _accountsGatewayMock.Setup(x => x.GetAccountsByRole("Admin")).Returns(new[] { new BusinessAccount() });
            _accountsGatewayMock.Setup(x => x.CreateAccount(It.Is<BusinessAccount>(a =>
                a.Description == "Created automatically"
                && a.Email == serviceAccount.Mail
                && a.FullName == serviceAccount.DisplayName
                && a.Name == serviceAccount.Name
                && a.Role.Name == "BasicUser"
                && a.ExternalId == serviceAccount.AccountId), false)).Returns(account);
            _accountsGatewayMock.Setup(x => x.StartSession(account, command.SessionId, 15)).Returns(session);
            _businessRuleEngineMock.Setup(x => x.Execute<int>(Guid.Empty, BusinessRuleConstants.Config.SessionDurationRule.ExternalId, null)).Returns(15);
            _accountsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _authenticationService.Verify(x => x.GetAccount(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.GetAccountByEmail(It.IsAny<string>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.CreateAccount(It.IsAny<BusinessAccount>(), It.IsAny<bool>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.StartSession(It.IsAny<BusinessAccount>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once);
            _businessRuleEngineMock.Verify(x => x.Execute<int>(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, object>>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldCreateAdminAccountAndStartSession_WhenUserAuthenticatedWithNotExistingAccountAndAdminNotExists()
        {
            var command = Builder<StartSessionCommand>.CreateNew().Build();
            var serviceAccount = Builder<Account>.CreateNew().Build();
            var account = Builder<BusinessAccount>.CreateNew().Build();
            var session = Builder<BusinessEntities.Auth.Session>.CreateNew().Build();

            _authenticationService.Setup(x => x.GetAccount(command.Login, command.Password)).Returns(serviceAccount);
            _accountsGatewayMock.Setup(x => x.GetAccountByEmail(serviceAccount.Mail)).Returns((BusinessAccount)null);
            _accountsGatewayMock.Setup(x => x.GetAccountsByRole("Admin")).Returns((IEnumerable<BusinessAccount>) null);
            _accountsGatewayMock.Setup(x => x.CreateAccount(It.Is<BusinessAccount>(a =>
                a.Description == "Created automatically"
                && a.Email == serviceAccount.Mail
                && a.FullName == serviceAccount.DisplayName
                && a.Name == serviceAccount.Name
                && a.Role.Name == "Admin"
                && a.ExternalId == serviceAccount.AccountId), false)).Returns(account);
            _accountsGatewayMock.Setup(x => x.StartSession(account, command.SessionId, 15)).Returns(session);
            _businessRuleEngineMock.Setup(x => x.Execute<int>(Guid.Empty, BusinessRuleConstants.Config.SessionDurationRule.ExternalId, null)).Returns(15);
            _accountsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _authenticationService.Verify(x => x.GetAccount(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.GetAccountByEmail(It.IsAny<string>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.CreateAccount(It.IsAny<BusinessAccount>(), It.IsAny<bool>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.StartSession(It.IsAny<BusinessAccount>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once);
            _businessRuleEngineMock.Verify(x => x.Execute<int>(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, object>>()), Times.Once);
        }
    }
}
