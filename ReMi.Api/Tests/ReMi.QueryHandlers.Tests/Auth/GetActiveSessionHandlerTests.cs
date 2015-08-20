using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessLogic.Auth;
using ReMi.Common.Utils;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.Auth;
using ReMi.QueryHandlers.Auth;
using BusinessSession = ReMi.BusinessEntities.Auth.Session;
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;

namespace ReMi.QueryHandlers.Tests.Auth
{
    public class GetActiveSessionHandlerTests : TestClassFor<GetAccountsHandler>
    {
        private Mock<IAccountsBusinessLogic> _accountsBusinessLogicMock;
        private Mock<IHandleQuery<GetAccountRequest, GetAccountResponse>> _getAccountQueryMock;

        protected override GetAccountsHandler ConstructSystemUnderTest()
        {
            _accountsBusinessLogicMock = new Mock<IAccountsBusinessLogic>();
            _getAccountQueryMock = new Mock<IHandleQuery<GetAccountRequest, GetAccountResponse>>();

            return new GetAccountsHandler
            {
                GetAccountQuery = _getAccountQueryMock.Object,
                AccountsBusinessLogic = _accountsBusinessLogicMock.Object
            };
        }

        [Test]
        public void Handle_ShouldGetSession_WhenInvoked()
        {
            var request = Builder<GetActiveSessionRequest>.CreateNew().Build();

            Sut.Handle(request);

            _accountsBusinessLogicMock.Verify(o => o.GetSession(It.Is<Guid>(x => x == request.SessionId)));
        }

        [Test]
        public void Handle_ShouldReturnNull_WhenSessionNotExists()
        {
            var session = Builder<BusinessSession>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            var request = Builder<GetActiveSessionRequest>.CreateNew()
                .With(o => o.SessionId, session.ExternalId)
                .Build();

            var result = Sut.Handle(request);

            Assert.IsNull(result);
        }

        [Test]
        public void Handle_ShouldReturnNull_WhenSessionExpired()
        {
            var session = Builder<BusinessSession>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ExpireAfter, SystemTime.Now.AddMinutes(-1))
                .Build();

            var request = Builder<GetActiveSessionRequest>.CreateNew()
                .With(o => o.SessionId, session.ExternalId)
                .Build();

            _accountsBusinessLogicMock.Setup(o => o.GetSession(It.Is<Guid>(x => x == request.SessionId)))
                .Returns(session);

            var result = Sut.Handle(request);

            Assert.IsNull(result);
        }

        [Test]
        public void Handle_ShouldReturnNull_WhenSessionCompleted()
        {
            var session = Builder<BusinessSession>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ExpireAfter, null)
                .With(o => o.Completed, SystemTime.Now)
                .Build();

            var request = Builder<GetActiveSessionRequest>.CreateNew()
                .With(o => o.SessionId, session.ExternalId)
                .Build();

            _accountsBusinessLogicMock.Setup(o => o.GetSession(It.Is<Guid>(x => x == request.SessionId)))
                .Returns(session);

            var result = Sut.Handle(request);

            Assert.IsNull(result);
        }

        [Test]
        public void Handle_ShouldGetAccount_WhenInvoked()
        {
            var session = Builder<BusinessSession>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.AccountId, Guid.NewGuid())
                .With(o => o.ExpireAfter, null)
                .With(o => o.Completed, null)
                .Build();

            var request = Builder<GetActiveSessionRequest>.CreateNew()
                .With(o => o.SessionId, session.ExternalId)
                .Build();

            _accountsBusinessLogicMock.Setup(o => o.GetSession(It.Is<Guid>(x => x == request.SessionId)))
                .Returns(session);

            _getAccountQueryMock.Setup(o => o.Handle(It.IsAny<GetAccountRequest>()))
                .Returns<GetAccountRequest>(
                    a => new GetAccountResponse {Account = new BusinessAccount {ExternalId = a.AccountId}});

            Sut.Handle(request);

            _getAccountQueryMock.Verify(o => o.Handle(It.IsAny<GetAccountRequest>()));
        }

        [Test]
        public void Handle_ShouldReturnNull_WhenAccountIsBlocked()
        {
            var session = Builder<BusinessSession>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.AccountId, Guid.NewGuid())
                .With(o => o.ExpireAfter, null)
                .With(o => o.Completed, null)
                .Build();
            var account = Builder<BusinessAccount>.CreateNew()
                .With(o => o.ExternalId, session.ExternalId)
                .With(o => o.IsBlocked, true)
                .Build();

            var request = Builder<GetActiveSessionRequest>.CreateNew()
                .With(o => o.SessionId, session.ExternalId)
                .Build();

            _accountsBusinessLogicMock.Setup(o => o.GetSession(It.Is<Guid>(x => x == request.SessionId)))
                .Returns(session);
            _getAccountQueryMock.Setup(o => o.Handle(It.IsAny<GetAccountRequest>()))
                .Returns<GetAccountRequest>(
                    a => new GetAccountResponse {Account = null});


            var result = Sut.Handle(request);

            Assert.IsNull(result);
        }

        [Test]
        public void Handle_ShouldInvokeMappers_WhenInvoked()
        {
            var session = Builder<BusinessSession>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.AccountId, Guid.NewGuid())
                .With(o => o.ExpireAfter, null)
                .With(o => o.Completed, null)
                .Build();
            var account = Builder<BusinessAccount>.CreateNew()
                .With(o => o.ExternalId, session.ExternalId)
                .Build();

            var request = Builder<GetActiveSessionRequest>.CreateNew()
                .With(o => o.SessionId, session.ExternalId)
                .Build();

            _accountsBusinessLogicMock.Setup(o => o.GetSession(It.Is<Guid>(x => x == request.SessionId)))
                .Returns(session);
            _getAccountQueryMock.Setup(o => o.Handle(It.IsAny<GetAccountRequest>()))
                .Returns<GetAccountRequest>(
                    a => new GetAccountResponse {Account = new BusinessAccount {ExternalId = a.AccountId}});

            Sut.Handle(request);
        }
    }
}
