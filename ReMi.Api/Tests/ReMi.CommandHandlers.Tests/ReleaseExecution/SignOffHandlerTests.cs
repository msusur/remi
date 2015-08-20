using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.BusinessLogic.Auth;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.CommandHandlers.ReleaseExecution;
using ReMi.Commands.DeploymentTool;
using ReMi.Commands.ReleaseExecution;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils.Enums;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.DataAccess.Exceptions;
using ReMi.Events.ReleaseExecution;

namespace ReMi.CommandHandlers.Tests.ReleaseExecution
{
    public class SignOffHandlerTests : TestClassFor<SignOffHandler>
    {
        private Mock<ISignOffGateway> _signOffGatewayMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<ICommandDispatcher> _commandDispatcher;
        private Mock<IPublishEvent> _eventPublisherMock;
        private ReleaseWindow _window;
        private List<Account> _accounts;
        private Mock<IAccountsBusinessLogic> _accountsBusinessLogicMock;
        private Mock<IReleaseWindowHelper> _releaseWindowHelperMock;

        protected override SignOffHandler ConstructSystemUnderTest()
        {
            return new SignOffHandler
            {
                AccountGatewayFactory = () => _accountsGatewayMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                SignOffGatewayFactory = () => _signOffGatewayMock.Object,
                CommandDispatcher = _commandDispatcher.Object,
                AccountsBusinessLogic = _accountsBusinessLogicMock.Object,
                EventPublisher = _eventPublisherMock.Object,
                ReleaseWindowHelper = _releaseWindowHelperMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _signOffGatewayMock = new Mock<ISignOffGateway>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _accountsGatewayMock = new Mock<IAccountsGateway>();
            _eventPublisherMock = new Mock<IPublishEvent>();
            _commandDispatcher = new Mock<ICommandDispatcher>();
            _accountsBusinessLogicMock = new Mock<IAccountsBusinessLogic>();

            _releaseWindowHelperMock = new Mock<IReleaseWindowHelper>();
            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(false);

            _window = new ReleaseWindow { Products = new[] { RandomData.RandomString(6) }, ExternalId = Guid.NewGuid(), ReleaseType = ReleaseType.Scheduled };
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(_window.ExternalId, true, It.IsAny<bool>())).Returns(_window);

            _accounts = new List<Account>
            {
                new Account {ExternalId = Guid.NewGuid(), Role = new Role{Description = RandomData.RandomString(22)}},
                new Account {ExternalId = Guid.NewGuid(), Role = new Role()}
            };
            _accountsGatewayMock.Setup(x => x.GetProductOwners(_window.Products)).Returns(_accounts);
            _accountsGatewayMock.Setup(account => account.GetAccounts(It.IsAny<IEnumerable<Guid>>()))
                .Returns(_accounts);

            _signOffGatewayMock.Setup(s => s.CheckSigningOff(_window.ExternalId)).Returns(false);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldAddSignOffsAndSendEvent_WhenCommandIsBackGround()
        {
            var command = new AddPeopleToSignOffReleaseCommand
            {
                ReleaseWindowId = _window.ExternalId,
                IsBackground = true
            };

            Sut.Handle(command);

            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(_window.ExternalId, true, It.IsAny<bool>()));
            _accountsGatewayMock.Verify(x => x.GetProductOwners(_window.Products));

            _signOffGatewayMock.Verify(s => s.AddSigners(command.SignOffs, command.ReleaseWindowId));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseSignersAddedEvent>(
                            r => r.SignOffs == command.SignOffs && r.ReleaseWindowId == command.ReleaseWindowId)));
        }

        [Test]
        [ExpectedException(typeof(AccountIsBlockedException))]
        public void HandleAddPeopleToSignOffRelease_ShouldThrowException_WhenAccountIsBlocked()
        {
            var command = new AddPeopleToSignOffReleaseCommand
            {
                ReleaseWindowId = _window.ExternalId,
                IsBackground = false,
                SignOffs = _accounts.Select(a => new SignOff { ExternalId = Guid.NewGuid(), Signer = a }).ToList()
            };

            var accounts = new List<Account> { new Account { ExternalId = _accounts.First().ExternalId, IsBlocked = true } };

            _accountsGatewayMock.Setup(account => account.GetAccounts(It.IsAny<IEnumerable<Guid>>()))
                .Returns(accounts);


            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldAddSignOffsAndSendEvent_WhenCommandIsFromFrontEnd()
        {
            var command = new AddPeopleToSignOffReleaseCommand
            {
                ReleaseWindowId = _window.ExternalId,
                IsBackground = false,
                SignOffs = _accounts.Select(a => new SignOff { ExternalId = Guid.NewGuid(), Signer = a }).ToList()
            };

            Sut.Handle(command);

            _accountsGatewayMock.Verify(
                a =>
                    a.CreateNotExistingAccounts(
                        It.Is<List<Account>>(
                            l =>
                                l.Count == 2 &&
                                l.Any(
                                    i =>
                                        i.Role.Description == "Product owner" && i.ExternalId == _accounts[1].ExternalId) &&
                                l.Any(
                                    i =>
                                        i.Role.Description == _accounts[0].Role.Description &&
                                        i.ExternalId == _accounts[0].ExternalId)), "ProductOwner"));
            _signOffGatewayMock.Verify(s => s.AddSigners(command.SignOffs, command.ReleaseWindowId));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseSignersAddedEvent>(
                            r => r.SignOffs == command.SignOffs && r.ReleaseWindowId == command.ReleaseWindowId)));
        }

        [Test]
        public void Handle_ShouldCallGatewayToSignOff()
        {
            var command = new SignOffReleaseCommand
            {
                AccountId = _accounts.First().ExternalId,
                ReleaseWindowId = _window.ExternalId,
                Comment = RandomData.RandomString(15)
            };
            var account = new Account { ExternalId = command.AccountId, IsBlocked = false };

            _accountsGatewayMock.Setup(acc => acc.GetAccount(command.AccountId, It.IsAny<bool>()))
                .Returns(account);

            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns(new Session { AccountId = command.AccountId });
            Sut.Handle(command);

            _signOffGatewayMock.Verify(s => s.SignOff(command.AccountId, command.ReleaseWindowId, command.Comment));
            _signOffGatewayMock.Verify(s => s.CheckSigningOff(_window.ExternalId));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseSignedOffBySignerEvent>(
                            r => r.ReleaseWindowGuid == command.ReleaseWindowId && r.AccountId == command.AccountId)));
        }

        [Test]
        [ExpectedException(typeof(AccountIsBlockedException))]
        public void Handle_ShouldThrowException_WhenAccountIsBlocked()
        {
            var command = new SignOffReleaseCommand
            {
                AccountId = _accounts.First().ExternalId,
                ReleaseWindowId = _window.ExternalId,
                Comment = RandomData.RandomString(15)
            };
            var account = new Account { ExternalId = command.AccountId, IsBlocked = true };

            _accountsGatewayMock.Setup(acc => acc.GetAccount(command.AccountId, It.IsAny<bool>()))
                .Returns(account);

            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns(new Session { AccountId = command.AccountId });

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldCallGatewayToSignOffAndPublishEvents_WhenReleaseIsFullySigned()
        {
            _signOffGatewayMock.Setup(s => s.CheckSigningOff(_window.ExternalId)).Returns(true);
            var command = new SignOffReleaseCommand
            {
                AccountId = _accounts.First().ExternalId,
                ReleaseWindowId = _window.ExternalId,
                Comment = RandomData.RandomString(15),
                UserName = RandomData.RandomEmail(),
                CommandContext = new CommandContext { UserId = Guid.NewGuid() },
                Password = RandomData.RandomString(15)
            };
            var session = new Session { AccountId = command.AccountId };
            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns(session);

            var account = new Account { ExternalId = command.AccountId, IsBlocked = false };

            _accountsGatewayMock.Setup(acc => acc.GetAccount(command.AccountId, It.IsAny<bool>()))
                .Returns(account);

            Sut.Handle(command);

            _accountsBusinessLogicMock.Verify(o => o.SignSession(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _signOffGatewayMock.Verify(s => s.SignOff(session.AccountId, command.ReleaseWindowId, command.Comment));
            _signOffGatewayMock.Verify(s => s.CheckSigningOff(_window.ExternalId));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseSignedOffBySignerEvent>(
                            r => r.ReleaseWindowGuid == command.ReleaseWindowId && r.AccountId == session.AccountId)));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseWindowSignedOffEvent>(
                            r => r.ReleaseWindow.ExternalId == command.ReleaseWindowId)));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseStatusChangedEvent>(
                            r =>
                                r.ReleaseWindowId == command.ReleaseWindowId &&
                                r.ReleaseStatus == EnumDescriptionHelper.GetDescription(ReleaseStatus.SignedOff))));
            _commandDispatcher.Verify(
               x =>
                   x.Send(
                       It.Is<PopulateDeploymentMeasurementsCommand>(c => c.ReleaseWindowId == command.ReleaseWindowId)));
        }

        [Test]
        [ExpectedException(typeof(FailedToAuthenticateException))]
        public void HandleSignOffRelease_ShouldThrowException_WhenFailedToAuthenticateSignature()
        {
            _signOffGatewayMock.Setup(s => s.CheckSigningOff(_window.ExternalId)).Returns(true);
            var command = new SignOffReleaseCommand
            {
                AccountId = Guid.NewGuid(),
                ReleaseWindowId = _window.ExternalId,
                Comment = RandomData.RandomString(15)
            };

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldCallGatewayToRemoveSigner()
        {
            var command = new RemoveSignOffCommand
            {
                SignOffId = Guid.NewGuid(),
                ReleaseWindowId = _window.ExternalId
            };

            Sut.Handle(command);

            _signOffGatewayMock.Verify(s => s.RemoveSigner(command.SignOffId));
            _signOffGatewayMock.Verify(s => s.CheckSigningOff(_window.ExternalId));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<RemoveSignOffEvent>(
                            r =>
                                r.ReleaseWindowGuid == command.ReleaseWindowId && r.SignOffId == command.SignOffId &&
                                r.AccountId == command.AccountId)));
        }

        [Test]
        public void Handle_ShouldCallGatewayToRemoveSignerAndPublishEvents_WhenReleaseIsFullySigned()
        {
            _signOffGatewayMock.Setup(s => s.CheckSigningOff(_window.ExternalId)).Returns(true);
            var command = new RemoveSignOffCommand
            {
                SignOffId = Guid.NewGuid(),
                ReleaseWindowId = _window.ExternalId,
                AccountId = Guid.NewGuid(),
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            };

            Sut.Handle(command);

            _signOffGatewayMock.Verify(s => s.RemoveSigner(command.SignOffId));
            _signOffGatewayMock.Verify(s => s.CheckSigningOff(_window.ExternalId));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<RemoveSignOffEvent>(
                            r =>
                                r.ReleaseWindowGuid == command.ReleaseWindowId && r.SignOffId == command.SignOffId &&
                                r.AccountId == command.AccountId)));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseWindowSignedOffEvent>(
                            r => r.ReleaseWindow.ExternalId == command.ReleaseWindowId)));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseStatusChangedEvent>(
                            r =>
                                r.ReleaseWindowId == command.ReleaseWindowId &&
                                r.ReleaseStatus == EnumDescriptionHelper.GetDescription(ReleaseStatus.SignedOff))));

            _commandDispatcher.Verify(
                x =>
                    x.Send(
                        It.Is<PopulateDeploymentMeasurementsCommand>(c => c.ReleaseWindowId == command.ReleaseWindowId)));
        }

        [Test]
        public void Handle_ShouldNotPopulateDeploymentMeasurements_WhenReleaseIsMaintenance()
        {
            _window = new ReleaseWindow { Products = new[] { RandomData.RandomString(6) }, ExternalId = Guid.NewGuid(), ReleaseType = ReleaseType.SystemMaintenance };
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(_window.ExternalId, true, It.IsAny<bool>())).Returns(_window);

            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(true);

            _signOffGatewayMock.Setup(s => s.CheckSigningOff(_window.ExternalId)).Returns(true);
            var command = new RemoveSignOffCommand
            {
                SignOffId = Guid.NewGuid(),
                ReleaseWindowId = _window.ExternalId,
                AccountId = Guid.NewGuid(),
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            };

            Sut.Handle(command);

            _commandDispatcher.Verify(
                x =>
                    x.Send(
                        It.Is<PopulateDeploymentMeasurementsCommand>(c => c.ReleaseWindowId == command.ReleaseWindowId)), Times.Never);
        }
    }
}
