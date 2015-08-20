using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.Auth;
using ReMi.QueryHandlers.Auth;

namespace ReMi.QueryHandlers.Tests.Auth
{
    public class GetNewSessionHandlerTests : TestClassFor<GetNewSessionHandler>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;

        protected override GetNewSessionHandler ConstructSystemUnderTest()
        {
            return new GetNewSessionHandler
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
        public void Handle_ShouldReturnNull_WhenSessionNotExists()
        {
            var request = new GetNewSessionRequest { SessionId = Guid.NewGuid() };
            _accountsGatewayMock.Setup(x => x.GetSession(request.SessionId)).Returns((Session)null);
            _accountsGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            Assert.IsNull(result);
            _accountsGatewayMock.Verify(x => x.GetSession(It.IsAny<Guid>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.GetAccount(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldReturnNull_WhenAccountNotExists()
        {
            var request = new GetNewSessionRequest { SessionId = Guid.NewGuid() };
            var session = new Session { AccountId = Guid.NewGuid() };
            _accountsGatewayMock.Setup(x => x.GetSession(request.SessionId)).Returns(session);
            _accountsGatewayMock.Setup(x => x.GetAccount(session.AccountId, true)).Returns((Account)null);
            _accountsGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            Assert.IsNull(result);
            _accountsGatewayMock.Verify(x => x.GetSession(It.IsAny<Guid>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.GetAccount(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldReturnSessionAndAccount_WhenSessionAndAccountExist()
        {
            var request = new GetNewSessionRequest { SessionId = Guid.NewGuid() };
            var session = new Session { AccountId = Guid.NewGuid() };
            var account = new Account();
            _accountsGatewayMock.Setup(x => x.GetSession(request.SessionId)).Returns(session);
            _accountsGatewayMock.Setup(x => x.GetAccount(session.AccountId, true)).Returns(account);
            _accountsGatewayMock.Setup(x => x.Dispose());

            var result = Sut.Handle(request);

            Assert.AreEqual(account, result.Account);
            Assert.AreEqual(session, result.Session);
            _accountsGatewayMock.Verify(x => x.GetSession(It.IsAny<Guid>()), Times.Once);
            _accountsGatewayMock.Verify(x => x.GetAccount(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
        }
    }
}
