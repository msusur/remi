using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.BusinessLogic.Auth;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Commands.DeploymentTool;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.Common.Utils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.Events.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;

namespace ReMi.BusinessLogic.Tests.ReleasePlan
{
    public class ReleaseWindowStateUpdaterTests : TestClassFor<ReleaseWindowStateUpdater>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<ISignOffGateway> _signOffGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private Mock<IBusinessRuleEngine> _businessRuleEngineMock;
        private Mock<IAccountsBusinessLogic> _accountsBusinessLogicMock;
        private Mock<IReleaseWindowHelper> _releaseWindowHelperMock;

        protected override ReleaseWindowStateUpdater ConstructSystemUnderTest()
        {
            return new ReleaseWindowStateUpdater
            {
                AccountsGatewayFactory = () => _accountsGatewayMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object,
                AccountsBusinessLogic = _accountsBusinessLogicMock.Object,
                BusinessRuleEngine = _businessRuleEngineMock.Object,
                SignOffGatewayFactory = () => _signOffGatewayMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object,
                ReleaseWindowHelper = _releaseWindowHelperMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountsGatewayMock = new Mock<IAccountsGateway>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _signOffGatewayMock = new Mock<ISignOffGateway>();
            _eventPublisherMock = new Mock<IPublishEvent>();
            _businessRuleEngineMock = new Mock<IBusinessRuleEngine>();
            _accountsBusinessLogicMock = new Mock<IAccountsBusinessLogic>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            
            _releaseWindowHelperMock = new Mock<IReleaseWindowHelper>();
            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(false);

            base.TestInitialize();
        }

        [Test]
        public void CloseRelease_ShouldCallGatewayToCreateNotExistingAccountForAddressee()
        {
            var address = new Account { Email = RandomData.RandomEmail() };
            var command = new CloseReleaseCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                ReleaseNotes = RandomData.RandomString(100),
                Recipients = new List<Account> { address },
                CommandContext = new CommandContext { UserId = Guid.NewGuid() },
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10),
            };
            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns(new Session { AccountId = command.CommandContext.UserId });
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(command.ReleaseWindowId, It.IsAny<bool>(), false))
                .Returns(new ReleaseWindow { ExternalId = command.ReleaseWindowId, SignedOff = SystemTime.Now });

            Sut.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId);

            _accountsGatewayMock.Verify(g => g.CreateNotExistingAccount(address));
            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public void CloseRelease_ShouldCallGatewayToCloseRelease()
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
                .Returns(new Session { AccountId = command.CommandContext.UserId });
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(command.ReleaseWindowId, It.IsAny<bool>(), false))
                .Returns(new ReleaseWindow { ExternalId = command.ReleaseWindowId, SignedOff = SystemTime.Now, ReleaseType = ReleaseType.Scheduled});

            Sut.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId);

            _releaseWindowGatewayMock.Verify(g => g.CloseRelease(command.ReleaseNotes, command.ReleaseWindowId));
        }

        [Test]
        public void CloseRelease_ShouldPublishCloseReleaseEvent()
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
                .Returns(new Session { AccountId = command.CommandContext.UserId });
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(command.ReleaseWindowId, It.IsAny<bool>(), false))
                .Returns(new ReleaseWindow { ExternalId = command.ReleaseWindowId, SignedOff = SystemTime.Now, ReleaseType = ReleaseType.Scheduled });

            Sut.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId);

            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseWindowClosedEvent>(c => c.Recipients == command.Recipients && c.ReleaseWindowId == command.ReleaseWindowId)));
        }

        [Test]
        public void CloseRelease_ShouldPublishStatusChangedEvent()
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
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(command.ReleaseWindowId, It.IsAny<bool>(), false))
                .Returns(new ReleaseWindow { ExternalId = command.ReleaseWindowId, SignedOff = SystemTime.Now, ReleaseType = ReleaseType.Scheduled });

            Sut.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId);

            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseStatusChangedEvent>(
                            c =>
                                c.ReleaseWindowId == command.ReleaseWindowId &&
                                c.ReleaseStatus == EnumDescriptionHelper.GetDescription(ReleaseStatus.Closed))));
        }

        [Test]
        [ExpectedException(typeof(ReleaseNotSignedOffException))]
        public void CloseRelease_ShouldNotCloseReleaseAndThrowException_WhenReleaseNotSignedOffAndNotAllowedToClose()
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
                .Returns(new Session { AccountId = command.CommandContext.UserId });
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(command.ReleaseWindowId, It.IsAny<bool>(), false))
                .Returns(new ReleaseWindow { ExternalId = command.ReleaseWindowId, ReleaseType = ReleaseType.Scheduled});
            _businessRuleEngineMock.Setup(x => x.Execute<bool>(command.CommandContext.UserId,
                It.IsAny<Guid>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(false);

            Sut.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId);
        }

        [Test]
        public void CloseRelease_ShouldCloseRelease_WhenMaintenanceReleaseNotSignedOff()
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
                .Returns(new Session { AccountId = command.CommandContext.UserId });
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(command.ReleaseWindowId, It.IsAny<bool>(), false))
                .Returns(new ReleaseWindow { ExternalId = command.ReleaseWindowId, ReleaseType = ReleaseType.Scheduled });
            _businessRuleEngineMock.Setup(x => x.Execute<bool>(command.CommandContext.UserId,
                It.IsAny<Guid>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(false);
            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(true);

            Sut.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId);

            _releaseWindowGatewayMock.Verify(g => g.CloseRelease(command.ReleaseNotes, command.ReleaseWindowId), Times.Once);
        }

        [Test]
        public void CloseRelease_ShouldNotSignOffPendingSignOffs_WhenSignOffListIsEmpty()
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
                .Returns(new Session { AccountId = command.CommandContext.UserId });
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(command.ReleaseWindowId, It.IsAny<bool>(), false))
                .Returns(new ReleaseWindow { ExternalId = command.ReleaseWindowId, ReleaseType = ReleaseType.Scheduled });
            _businessRuleEngineMock.Setup(x => x.Execute<bool>(command.CommandContext.UserId,
                It.IsAny<Guid>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(true);

            Sut.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId);

            _releaseWindowGatewayMock.Verify(g => g.CloseRelease(command.ReleaseNotes, command.ReleaseWindowId), Times.Once);
            _signOffGatewayMock.Verify(x => x.CheckSigningOff(command.ReleaseWindowId), Times.Never);
        }

        [Test]
        public void CloseRelease_ShouldSignOffAllPendingSignOffs_WhenReleaseIsNotYetSignOff()
        {
            var address = new Account { Email = RandomData.RandomEmail() };
            var signOffs = Builder<SignOff>.CreateListOfSize(5)
                .All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Do(x => x.SignedOff = RandomData.RandomBool())
                .Do(x => x.Signer = new Account { ExternalId = Guid.NewGuid() })
                .Build();
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
                .Returns(new Session { AccountId = command.CommandContext.UserId });
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(command.ReleaseWindowId, It.IsAny<bool>(), false))
                .Returns(new ReleaseWindow { ExternalId = command.ReleaseWindowId, ReleaseType = ReleaseType.Scheduled});
            _signOffGatewayMock.Setup(x => x.GetSignOffs(command.ReleaseWindowId))
                .Returns((List<SignOff>)signOffs);
            _signOffGatewayMock.Setup(x => x.SignOff(It.IsAny<Guid>(), It.IsAny<Guid>(), "Automated sign off during release closing"))
                .Callback((Guid accountId, Guid releaseWindowid, string s) => signOffs.First(x => x.Signer.ExternalId == accountId).SignedOff = true);

            _businessRuleEngineMock.Setup(x => x.Execute<bool>(command.CommandContext.UserId,
                BusinessRuleConstants.Release.AllowCloseAfterSignOffRule.ExternalId, It.IsAny<IDictionary<string, object>>()))
                .Returns(true);

            Sut.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId);

            _releaseWindowGatewayMock.Verify(g => g.CloseRelease(command.ReleaseNotes, command.ReleaseWindowId), Times.Once);
            _signOffGatewayMock.Verify(x => x.CheckSigningOff(command.ReleaseWindowId), Times.Once);
            Assert.IsTrue(signOffs.All(x => x.SignedOff));
        }

        [Test]
        public void CloseRelease_ShouldPopulateDeploymentJobMetrics_WhenSigningOffPendingSignOffs()
        {
            var address = new Account { Email = RandomData.RandomEmail() };
            var signOffs = Builder<SignOff>.CreateListOfSize(5)
                .All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Do(x => x.SignedOff = RandomData.RandomBool())
                .Do(x => x.Signer = new Account { ExternalId = Guid.NewGuid() })
                .Build();
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
                .Returns(new Session { AccountId = command.CommandContext.UserId });
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(command.ReleaseWindowId, It.IsAny<bool>(), false))
                .Returns(new ReleaseWindow { ExternalId = command.ReleaseWindowId, ReleaseType = ReleaseType.Scheduled});
            _signOffGatewayMock.Setup(x => x.GetSignOffs(command.ReleaseWindowId))
                .Returns((List<SignOff>)signOffs);
            _signOffGatewayMock.Setup(x => x.SignOff(It.IsAny<Guid>(), It.IsAny<Guid>(), "Automated sign off during release closing"))
                .Callback((Guid accountId, Guid releaseWindowid, string s) => signOffs.First(x => x.Signer.ExternalId == accountId).SignedOff = true);
            _businessRuleEngineMock.Setup(x => x.Execute<bool>(command.CommandContext.UserId,
                BusinessRuleConstants.Release.AllowCloseAfterSignOffRule.ExternalId, It.IsAny<IDictionary<string, object>>()))
                .Returns(true);

            Sut.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId);

            _commandDispatcherMock.Verify(g => g.Send(It.IsAny<PopulateDeploymentMeasurementsCommand>()), Times.Once);
        }
    }
}
