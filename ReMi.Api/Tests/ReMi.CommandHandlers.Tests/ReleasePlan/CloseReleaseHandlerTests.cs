using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.Auth;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class CloseReleaseHandlerTests : TestClassFor<CloseReleaseHandler>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<IReleaseWindowStateUpdater> _releaseWindowStateUpdaterMock;
        private Mock<IAccountsBusinessLogic> _accountsBusinessLogicMock;

        protected override CloseReleaseHandler ConstructSystemUnderTest()
        {
            return new CloseReleaseHandler
            {
                AccountsGatewayFactory = () => _accountsGatewayMock.Object,
                AccountsBusinessLogic = _accountsBusinessLogicMock.Object,
                ReleaseWindowStateUpdater = _releaseWindowStateUpdaterMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountsGatewayMock = new Mock<IAccountsGateway>();
            _releaseWindowStateUpdaterMock = new Mock<IReleaseWindowStateUpdater>();
            _accountsBusinessLogicMock = new Mock<IAccountsBusinessLogic>();

            base.TestInitialize();
        }

        #region CloseRelease

        [Test]
        [ExpectedException(typeof(FailedToAuthenticateException))]
        public void CloseRelease_ShouldThrowException_WhenFailedToAuthenticateSignature()
        {
            var address = new Account { Email = RandomData.RandomEmail() };
            var command = new CloseReleaseCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                ReleaseNotes = RandomData.RandomString(100),
                Recipients = new List<Account> { address },
                CommandContext = new CommandContext { UserId = Guid.NewGuid() },
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10)
            };
            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns((Session) null);

            Sut.Handle(command);
        }

        [Test]
        [ExpectedException(typeof(AttemptToSignAsOtherAccountException))]
        public void CloseRelease_ShouldThrowException_WhenAttemptingToSignFromOtherAccount()
        {
            var address = new Account { Email = RandomData.RandomEmail() };
            var command = new CloseReleaseCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                ReleaseNotes = RandomData.RandomString(100),
                Recipients = new List<Account> { address },
                CommandContext = new CommandContext { UserId = Guid.NewGuid() },
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10)
            };
            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns(new Session { AccountId = Guid.NewGuid() });

            Sut.Handle(command);
        }

        [Test]
        public void CloseRelease_ShouldChangeStatusChangedToChanged()
        {
            var address = new Account { Email = RandomData.RandomEmail() };
            var addressees = new List<Account> { address };
            var releaseNotes = RandomData.RandomString(100);
            var releaseWindowId = Guid.NewGuid();
            var command = new CloseReleaseCommand
            {
                ReleaseWindowId = releaseWindowId,
                ReleaseNotes = releaseNotes,
                Recipients = addressees,
                CommandContext = new CommandContext { UserId = Guid.NewGuid() },
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10)
            };
            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns(new Session { AccountId = command.CommandContext.UserId });

            Sut.Handle(command);

            _releaseWindowStateUpdaterMock.Verify(
                e =>
                    e.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId));
        }

        #endregion
    }
}
