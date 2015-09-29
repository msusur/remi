using System;
using System.Collections.Generic;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.Auth;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Contracts.Plugins.Services.Authentication;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.Exceptions;
using ReMi.TestUtils.UnitTests;
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;
using ContractAccount = ReMi.Contracts.Plugins.Data.Authentication.Account;
using System.Linq;
using ReMi.BusinessEntities.Products;
using ReMi.Common.Constants.BusinessRules;

namespace ReMi.BusinessLogic.Tests.Auth
{
    [TestFixture]
    public class AccountsBusinessLogicTests : TestClassFor<AccountsBusinessLogic>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<IProductGateway> _productGatewayMock;
        private Mock<IAuthenticationService> _authenticationServiceMock;
        private Mock<IMappingEngine> _mapperMock;
        private Mock<IBusinessRuleEngine> _businessRuleEngineMock;

        protected override void TestInitialize()
        {
            _accountsGatewayMock = new Mock<IAccountsGateway>();
            _productGatewayMock = new Mock<IProductGateway>();
            _businessRuleEngineMock = new Mock<IBusinessRuleEngine>(MockBehavior.Strict);
            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _mapperMock = new Mock<IMappingEngine>();

            base.TestInitialize();
        }

        protected override AccountsBusinessLogic ConstructSystemUnderTest()
        {
            return new AccountsBusinessLogic
            {
                AccountsGatewayFactory = () => _accountsGatewayMock.Object,
                ProductGatewayFactory = () => _productGatewayMock.Object,
                AuthenticationService = _authenticationServiceMock.Object,
                Mapper = _mapperMock.Object,
                BusinessRuleEngine = _businessRuleEngineMock.Object
            };
        }

        [Test]
        public void GetSession_ShouldCallGatewayGetSession_WhenInvoked()
        {
            var sessionId = new Guid();

            Sut.GetSession(sessionId);

            _accountsGatewayMock.Verify(o => o.GetSession(It.Is<Guid>(x => x == sessionId)));
        }

        [Test]
        public void GetSession_ShouldReturnNull_WhenInvokedWithNonExistingId()
        {
            var sessionId = new Guid();

            var session = Sut.GetSession(sessionId);

            Assert.IsNull(session);
        }

        [Test]
        public void SignSession_ShouldCallAccountValidation_WhenInvoked()
        {
            var accountName = RandomData.RandomString(1, 20);
            var password = RandomData.RandomString(1, 20);

            Sut.SignSession(accountName, password);

            _authenticationServiceMock.Verify(o => o.GetAccount(accountName, password));
        }

        [Test]
        public void SignSession_ShouldCallSignSession_WhenInvoked()
        {
            var accountName = RandomData.RandomString(1, 20);
            var password = RandomData.RandomString(1, 20);
            var email = RandomData.RandomEmail();

            SetupContractAccount(accountName, accountName, email);
            var account = SetupAccount(email, new Role() { Name = "ExecutiveManager" });

            Sut.SignSession(accountName, password);

            _accountsGatewayMock.Verify(o => o.SignSession(account));
        }
        [Test]
        public void SearchAccountTest()
        {
            var criteria = "criteria";
            var account = SetupContractAccount();
            var businessAccount = SetupAccount();
            var accounts = new List<ContractAccount> { account };

            _authenticationServiceMock.Setup(x => x.Search(criteria)).Returns(accounts);
            _accountsGatewayMock.Setup(acc => acc.GetAccounts()).Returns(new List<BusinessAccount> { businessAccount });

            var result = Sut.SearchAccounts(criteria);

            _authenticationServiceMock.Verify(x => x.Search(criteria));
            _accountsGatewayMock.Verify(acc => acc.GetAccounts());

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count, "wrong number of accounts");
            Assert.AreEqual(account.Mail, result[0].Email, "email");
            Assert.AreEqual(account.DisplayName, result[0].FullName, "fullname");
            Assert.AreEqual("BasicUser", result[0].Role.Name, "role");
        }


        [Test]
        [ExpectedException(typeof(AccountEmailNotFoundException))]
        public void SignSession_ShouldThrowException_WhenAccountNotFound()
        {
            var accountName = RandomData.RandomString(1, 20);
            var password = RandomData.RandomString(1, 20);
            var email = RandomData.RandomEmail();

            SetupContractAccount(accountName, accountName, email);

            Sut.SignSession(accountName, password);
        }


        [Test]
        [ExpectedException(typeof(AccountIsBlockedException))]
        public void SignSession_ShouldThrowException_WhenAccountIsBlocked()
        {
            var accountName = RandomData.RandomString(1, 20);
            var password = RandomData.RandomString(1, 20);
            var email = RandomData.RandomEmail();

            SetupContractAccount(accountName, accountName, email);
            SetupAccount(email, new Role { Name = "ExecutiveManager" }, true);

            Sut.SignSession(accountName, password);
        }

        [Test]
        public void AssociateAccountsWithProduct_ShouldGetPackagesAndAssociateThemWithAccount_WhenPackagesExist()
        {
            var accountsEmails = Enumerable.Repeat(RandomData.RandomEmail(), 10);
            var releaseWindowId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var packages = Enumerable.Repeat(new Product {ExternalId = Guid.NewGuid()}, 5);
            var result = new TeamRoleRuleResult();
            var roleName = RandomData.RandomString(10);

            _productGatewayMock.Setup(x => x.GetProducts(releaseWindowId))
                .Returns(packages);
            _productGatewayMock.Setup(x => x.Dispose());
            _accountsGatewayMock.Setup(x => x.AssociateAccountsWithProducts(
                It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(packages.Select(p => p.ExternalId))),
                accountsEmails,
                It.Is<Func<string, TeamRoleRuleResult>>(f => f(roleName) == result)));
            _accountsGatewayMock.Setup(x => x.Dispose());
            _businessRuleEngineMock.Setup(x => x.Execute<TeamRoleRuleResult>(
                userId,
                BusinessRuleConstants.Config.TeamRoleRule.ExternalId,
                It.Is<IDictionary<string, object>>(d => ((string)d["roleName"]) == roleName)))
            .Returns(result);

            Sut.AssociateAccountsWithProduct(accountsEmails, releaseWindowId, userId);

            _productGatewayMock.Verify(x => x.GetProducts(It.IsAny<Guid>()), Times.Once);
            _productGatewayMock.Verify(x => x.Dispose(), Times.Once);
            _accountsGatewayMock.Verify(x => x.AssociateAccountsWithProducts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<Func<string, TeamRoleRuleResult>>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.Dispose(), Times.Once);
        }

        #region Helpers

        private ContractAccount SetupContractAccount(string name = null, string displayName = null, string email = null)
        {
            var account = Builder<ContractAccount>.CreateNew()
                .With(o => o.Mail, email ?? RandomData.RandomEmail())
                .With(o => o.Name, name ?? RandomData.RandomString(1, 100))
                .With(o => o.DisplayName, displayName ?? RandomData.RandomString(1, 100))
                .Build();

            _authenticationServiceMock.Setup(o => o.GetAccount(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(account);

            return account;
        }

        private BusinessAccount SetupAccount(string email = null)
        {
            return SetupAccount(email, new Role { Name = "NotAuthenticated" });
        }
        private BusinessAccount SetupAccount(string email, Role role, bool isBlocked = false)
        {
            var account = Builder<BusinessAccount>.CreateNew()
                .With(o => o.Role, role.Name != "NotAuthenticated" ? role : new Role { Name = new[] { "Admin", "BasicUser", "NotAuthenticated", "ProductOwner" }[RandomData.RandomInt(0, 3)] })
                .With(o => o.Email, email ?? RandomData.RandomEmail())
                .With(o => o.IsBlocked, isBlocked)
                .Build();

            _accountsGatewayMock.Setup(o => o.GetAccountByEmail(account.Email))
                .Returns(account);

            return account;
        }

        #endregion
    }
}
