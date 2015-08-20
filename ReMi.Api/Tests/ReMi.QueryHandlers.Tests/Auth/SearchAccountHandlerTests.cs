using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.Auth;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryHandlers.ReleasePlan;
using System;
using System.Collections.Generic;

namespace ReMi.QueryHandlers.Tests.Auth
{
    public class SearchAccountHandlerTests : TestClassFor<SearchAccountHandler>
    {
        private Mock<IAccountsBusinessLogic> _accountBusinessLogicMock;
        private const string Criteria = "criteria";
        private Account _account;

        protected override void TestInitialize()
        {
            _account = new Account {ExternalId = Guid.NewGuid()};
            _accountBusinessLogicMock = new Mock<IAccountsBusinessLogic>();
            _accountBusinessLogicMock.Setup(acc => acc.SearchAccounts(Criteria))
                .Returns(new List<Account> {_account});

            base.TestInitialize();
        }

        protected override SearchAccountHandler ConstructSystemUnderTest()
        {
            return new SearchAccountHandler {AccountsBusinessLogicFactory = () => _accountBusinessLogicMock.Object};
        }

        [Test]
        public void Handle_SearchAccountRequest()
        {
            var result = Sut.Handle(new SearchAccountRequest {Criteria = Criteria});

            _accountBusinessLogicMock.Verify(acc => acc.SearchAccounts(Criteria));

            Assert.IsNotNull(result);
            Assert.AreEqual(_account.ExternalId, result.Accounts[0].ExternalId);
        }
    }
}
