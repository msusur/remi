using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.Exceptions;
using ReMi.CommandHandlers.Auth;
using ReMi.Commands.Auth;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Auth;

namespace ReMi.CommandHandlers.Tests.Configuration
{
    public class DefaultProductHandlerTests : TestClassFor<DefaultProductHandler>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;

        protected override DefaultProductHandler ConstructSystemUnderTest()
        {
            return new DefaultProductHandler
            {
                AccountsGatewayFactory = () => _accountsGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountsGatewayMock = new Mock<IAccountsGateway>();
            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof (UserAlreadyRegistedException))]
        public void Handle_ShouldThrowException_WhenUserAlreadyLoggedInFewTimes()
        {
            var command = new SetDefaultProductForNewlyRegisteredUserCommand
            {
                Account = new Account {ExternalId = Guid.NewGuid()}
            };
            var sessions = new List<Session>
            {
                new Session(),
                new Session()
            };
            _accountsGatewayMock.Setup(x => x.GetSessions(command.Account.ExternalId)).Returns(sessions);

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenUserAlreadyDidNotLogInFewTimes()
        {
            var command = new SetDefaultProductForNewlyRegisteredUserCommand
            {
                Account = new Account { ExternalId = Guid.NewGuid() }
            };
            var sessions = new List<Session>
            {
                new Session()
            };
            _accountsGatewayMock.Setup(x => x.GetSessions(command.Account.ExternalId)).Returns(sessions);

            Sut.Handle(command);

            _accountsGatewayMock.Verify(x => x.GetSessions(command.Account.ExternalId));

            _accountsGatewayMock.Verify(x => x.UpdateAccountProducts(command.Account));
        }
    }
}
