using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ContinuousDelivery;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic.Auth;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.CommandHandlers.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Commands.ReleasePlan;
using ReMi.Commands.SourceControl;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Constants.ContinuousDelivery;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils.Enums;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.Exceptions;
using ReMi.Events.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;
using ReMi.Events.ReleasePlan;
using ReMi.Queries.ContinuousDelivery;
using ReMi.Queries.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class ApproveReleaseHandlerTests : TestClassFor<ApproveReleaseHandler>
    {
        private Mock<IReleaseApproverGateway> _releaseApproverGatewayMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<IProductGateway> _productGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private Mock<IBusinessRuleEngine> _businessRuleEngineMock;
        private Mock<IAccountsBusinessLogic> _accountsBusinessLogicMock;
        private Mock<IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse>>
            _releaseContentQueryMock;

        private Mock<IHandleQuery<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse>>
            _getStatusQueryMock;

        protected override ApproveReleaseHandler ConstructSystemUnderTest()
        {
            return new ApproveReleaseHandler
            {
                ReleaseApproverGateway = () => _releaseApproverGatewayMock.Object,
                ReleaseWindowGateway = () => _releaseWindowGatewayMock.Object,
                AccountsGateway = () => _accountsGatewayMock.Object,
                ProductGateway = () => _productGatewayMock.Object,
                PublishEvent = _eventPublisherMock.Object,
                AccountsBusinessLogic = _accountsBusinessLogicMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object,
                GetQaStatus = _getStatusQueryMock.Object,
                BusinessRuleEngine = _businessRuleEngineMock.Object,
                GetReleaseContentInformationQuery = _releaseContentQueryMock.Object,
            };
        }

        protected override void TestInitialize()
        {
            _releaseApproverGatewayMock = new Mock<IReleaseApproverGateway>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _eventPublisherMock = new Mock<IPublishEvent>();
            _accountsBusinessLogicMock = new Mock<IAccountsBusinessLogic>();
            _accountsGatewayMock = new Mock<IAccountsGateway>();
            _productGatewayMock = new Mock<IProductGateway>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            _businessRuleEngineMock = new Mock<IBusinessRuleEngine>();
            _getStatusQueryMock = new Mock<IHandleQuery<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse>>();

            _releaseContentQueryMock =
                new Mock<IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse>>();

            _releaseContentQueryMock.Setup(x => x.Handle(It.IsAny<GetReleaseContentInformationRequest>()))
                .Returns(new GetReleaseContentInformationResponse
                {
                    Content = new List<ReleaseContentTicket>
                    {
                        new ReleaseContentTicket
                        {
                            IncludeToReleaseNotes = true,
                            TicketId = Guid.NewGuid(),
                            TicketDescription = RandomData.RandomString(5, 11)
                        },
                        new ReleaseContentTicket
                        {
                            TicketId = Guid.NewGuid(),
                            TicketDescription = RandomData.RandomString(15, 21)
                        }
                    }
                });

            _getStatusQueryMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()))
                .Returns(new GetContinuousDeliveryStatusResponse
                {
                    StatusCheck = new List<StatusCheckItem>
                    {
                        new StatusCheckItem
                        {
                            MetricControl = "test",
                            Status = StatusType.Green
                        }
                    }
                });

            base.TestInitialize();
        }


        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void Handle_AddApproversShouldThrowException_WhenInvokedWithEmtyAccount()
        {
            Sut.Handle(new AddReleaseApproversCommand { Approvers = new[] { new ReleaseApprover() } });
        }

        [Test]
        public void Handle_AddApproversShouldSetDefaultProductForNewAccount_WhenApproverIsNotInDatabase()
        {
            var account = Builder<Account>.CreateNew().With(o => o.ExternalId, Guid.NewGuid()).Build();
            var releaseWindowId = Guid.NewGuid();
            var products = Builder<Product>.CreateListOfSize(1).Build();

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(releaseWindowId, false, false))
                .Returns(new ReleaseWindow { ExternalId = releaseWindowId });

            _productGatewayMock.Setup(o => o.GetProducts(releaseWindowId)).Returns(products);

            Sut.Handle(new AddReleaseApproversCommand
            {
                Approvers = new[] { new ReleaseApprover { Account = account, ReleaseWindowId = releaseWindowId } }
            });

            _productGatewayMock.Verify(o => o.GetProducts(releaseWindowId));
        }

        [Test]
        public void Handle_AddApproversShouldAddAccount_WhenApproverIsNotInDatabase()
        {
            var account = Builder<Account>.CreateNew().With(o => o.ExternalId, Guid.NewGuid()).Build();
            var releaseWindowId = Guid.NewGuid();
            var products = Builder<Product>.CreateListOfSize(1).Build();

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(releaseWindowId, false, false))
                .Returns(new ReleaseWindow { ExternalId = releaseWindowId });

            _productGatewayMock.Setup(o => o.GetProducts(releaseWindowId)).Returns(products);

            Sut.Handle(new AddReleaseApproversCommand { Approvers = new[] { new ReleaseApprover { Account = account, ReleaseWindowId = releaseWindowId } } });

            _accountsGatewayMock.Verify(o => o.CreateAccount(account, true));
        }

        [Test]
        [ExpectedException(typeof(AccountIsBlockedException))]
        public void Handle_ShouldThrowException_WhenApproverAccountIsBlocked()
        {
            var account = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.IsBlocked, true)
                .Build();
            var releaseWindowId = Guid.NewGuid();
            var products = Builder<Product>.CreateListOfSize(1).Build();

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(releaseWindowId, false, false))
                .Returns(new ReleaseWindow { ExternalId = releaseWindowId });

            _productGatewayMock.Setup(o => o.GetProducts(releaseWindowId)).Returns(products);

            _accountsGatewayMock.Setup(o => o.GetAccount(account.ExternalId, It.IsAny<bool>()))
                .Returns(account);

            Sut.Handle(new AddReleaseApproversCommand { Approvers = new[] { new ReleaseApprover { Account = account, ReleaseWindowId = releaseWindowId } } });
        }

        [Test]
        public void Handle_AddApproversShouldInvokeGatewayAddApprover_WhenInvoked()
        {
            var accounts = Enumerable.Range(1, RandomData.RandomInt(1, 5))
                .Select(x => Builder<Account>.CreateNew().With(o => o.ExternalId, Guid.NewGuid()).Build()).ToArray();
            var releaseWindowId = Guid.NewGuid();

            foreach (var account in accounts)
            {
                var id = account.ExternalId;
                _accountsGatewayMock.Setup(o => o.GetAccount(id, false)).Returns(account);
            }

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(releaseWindowId, false, false))
                .Returns(new ReleaseWindow { ExternalId = releaseWindowId });


            Sut.Handle(new AddReleaseApproversCommand
            {
                Approvers =
                    accounts.Select(x => new ReleaseApprover { Account = x, ReleaseWindowId = releaseWindowId }).ToArray()
            });

            foreach (var account in accounts)
            {
                var acc = account;
                _releaseApproverGatewayMock.Verify(
                    o =>
                        o.AddApprover(
                            It.Is<ReleaseApprover>(r => r.Account == acc && r.ReleaseWindowId == releaseWindowId)));
            }
        }

        [Test]
        public void Handle_AddApproversShouldPublishApproverAddedToReleaseWindowEvent_WhenInvoked()
        {
            var accounts = Enumerable.Range(1, RandomData.RandomInt(1, 5))
                .Select(x => Builder<Account>.CreateNew().With(o => o.ExternalId, Guid.NewGuid()).Build()).ToArray();
            var releaseWindowId = Guid.NewGuid();


            _releaseWindowGatewayMock.Setup(o => o.GetByExternalId(releaseWindowId, false, It.IsAny<bool>()))
                .Returns(Builder<ReleaseWindow>.CreateNew()
                    .With(o => o.ExternalId, releaseWindowId)
                    .Build());

            foreach (var account in accounts)
            {
                var id = account.ExternalId;
                _accountsGatewayMock.Setup(o => o.GetAccount(id, false)).Returns(account);
            }

            Sut.Handle(new AddReleaseApproversCommand
            {
                Approvers =
                    accounts.Select(x => new ReleaseApprover { Account = x, ReleaseWindowId = releaseWindowId }).ToArray()
            });

            foreach (var account in accounts)
            {
                _eventPublisherMock.Verify(o => o.Publish(
                    It.Is<ApproverAddedToReleaseWindowEvent>(
                        x =>
                            x.Approver.Account.ExternalId == account.ExternalId &&
                            x.Approver.ReleaseWindowId == releaseWindowId)));
            }
        }

        [Test]
        public void Handle_ApproveShouldInvokeGatewayApproveReleaseAndSendApprovementEvent_WhenInvoked()
        {
            var accountId = Guid.NewGuid();
            var releaseWindowId = Guid.NewGuid();
            var command = new ApproveReleaseCommand
            {
                AccountId = accountId,
                ReleaseWindowId = releaseWindowId,
                CommandContext = new CommandContext(),
                Comment = "super desc",
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10)
            };

            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns(new Session { AccountId = command.AccountId });
            _releaseWindowGatewayMock.Setup(o => o.GetByExternalId(releaseWindowId, true, It.IsAny<bool>()))
                .Returns(Builder<ReleaseWindow>.CreateNew()
                    .With(o => o.ExternalId, releaseWindowId)
                    .Build());
            _accountsGatewayMock.Setup(o => o.GetAccount(command.AccountId, true))
                .Returns(new Account { ExternalId = command.AccountId });

            var approvers = new[]
            {
                new ReleaseApprover
                {
                    Account = new Account {Role = new Role {Name = "ProductOwner"}, ExternalId = accountId},
                    ApprovedOn = DateTime.Now
                },
                new ReleaseApprover
                {
                    Account = new Account {Role = new Role {Name = "ReleaseEngineer"}, ExternalId = Guid.NewGuid()},
                    ApprovedOn = DateTime.Now
                }
            };

            _releaseApproverGatewayMock.Setup(o => o.GetApprovers(releaseWindowId)).Returns(approvers);

            Sut.Handle(command);

            _releaseApproverGatewayMock.Verify(o => o.ApproveRelease(accountId, releaseWindowId, "super desc"));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ApprovementEvent>(
                            a => a.Comment == "super desc" && a.ReleaseWindowId == releaseWindowId)));
        }

        [Test]
        [ExpectedException(typeof(AccountIsBlockedException))]
        public void Handle_ApproveShouldApproveRelease_WhenReleaseFullyApproved()
        {
            var accountId = Guid.NewGuid();
            var releaseWindowId = Guid.NewGuid();
            var command = new ApproveReleaseCommand
            {
                AccountId = accountId,
                ReleaseWindowId = releaseWindowId,
                CommandContext = new CommandContext(),
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10)
            };

            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns(new Session { AccountId = command.AccountId });
            _accountsGatewayMock.Setup(o => o.GetAccount(command.AccountId, true))
                .Returns(new Account { ExternalId = command.AccountId, IsBlocked = true });

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ApproveShouldPublishReleaseWindowApprovedEvent_WhenReleaseFullyApproved()
        {
            var accountId = Guid.NewGuid();
            var releaseWindowId = Guid.NewGuid();

            _releaseApproverGatewayMock.Setup(o => o.GetApprovers(releaseWindowId)).Returns(new[]
                {
                    new ReleaseApprover {Account = new Account {Role = new Role { Name = "ProductOwner" }, ExternalId = accountId}, ApprovedOn = DateTime.Now},
                    new ReleaseApprover {Account = new Account {Role = new Role { Name = "ReleaseEngineer" } }, ApprovedOn = DateTime.Now}
                });
            var command = new ApproveReleaseCommand
            {
                AccountId = accountId,
                ReleaseWindowId = releaseWindowId,
                CommandContext = new CommandContext(),
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10)
            };

            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns(new Session { AccountId = command.AccountId });
            _accountsGatewayMock.Setup(o => o.GetAccount(command.AccountId, true))
                .Returns(new Account { ExternalId = command.AccountId });
            _releaseWindowGatewayMock.Setup(o => o.GetByExternalId(releaseWindowId, true, It.IsAny<bool>()))
                .Returns(Builder<ReleaseWindow>.CreateNew()
                    .With(o => o.ExternalId, releaseWindowId)
                    .With(o => o.ReleaseType, ReleaseType.Hotfix)
                    .Build());

            _businessRuleEngineMock.Setup(o => o.Execute<bool>(Guid.Empty,
                    BusinessRuleConstants.Release.ReleaseApprovalRule.ExternalId, It.IsAny<IDictionary<string, object>>()))
                .Returns(true);
            _productGatewayMock.Setup(x => x.GetProducts(releaseWindowId))
                .Returns(new[] { new Product { ReleaseTrack = ReleaseTrack.Manual } });

            Sut.Handle(command);

            _eventPublisherMock.Verify(
                o =>
                    o.Publish(It.Is<ReleaseWindowApprovedEvent>(evnt => evnt.ReleaseWindow.ExternalId == releaseWindowId)));
            _eventPublisherMock.Verify(
                o =>
                    o.Publish(
                        It.Is<ReleaseStatusChangedEvent>(
                            evnt =>
                                evnt.ReleaseWindowId == releaseWindowId && evnt.ReleaseStatus == EnumDescriptionHelper.GetDescription(ReleaseStatus.Approved))));
            _commandDispatcherMock.Verify(
                x => x.Send(It.Is<PersistTicketsCommand>(c => c.ReleaseWindowId == releaseWindowId)), Times.Never);
            _commandDispatcherMock.Verify(
                x => x.Send(It.Is<StoreSourceControlChangesCommand>(c => c.ReleaseWindowId == releaseWindowId)), Times.Never);
        }

        [Test]
        public void Handle_ShouldCallWindowGateway_WhenApproverRemoved()
        {
            var command = new RemoveReleaseApproverCommand
            {
                ApproverId = Guid.NewGuid(),
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext()
            };
            var releaseWindow = new ReleaseWindow { ExternalId = command.ReleaseWindowId };
            _releaseWindowGatewayMock.Setup(r => r.GetByExternalId(releaseWindow.ExternalId, false, It.IsAny<bool>())).Returns(releaseWindow);
            _releaseApproverGatewayMock.Setup(r => r.GetApprovers(releaseWindow.ExternalId))
                .Returns(new List<ReleaseApprover>());

            Sut.Handle(command);

            _releaseWindowGatewayMock.Verify(r => r.GetByExternalId(releaseWindow.ExternalId, false, It.IsAny<bool>()));
            _releaseApproverGatewayMock.Verify(r => r.RemoveApprover(command.ApproverId));
            _releaseWindowGatewayMock.Verify(r => r.ApproveRelease(releaseWindow.ExternalId), Times.Never);
        }

        [Test]
        public void Handle_ShouldPublishRemoveApproverEvent_WhenApproverRemoved()
        {
            var command = new RemoveReleaseApproverCommand
            {
                ApproverId = Guid.NewGuid(),
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext()
            };
            var releaseWindow = new ReleaseWindow { ExternalId = command.ReleaseWindowId };
            _releaseWindowGatewayMock.Setup(r => r.GetByExternalId(releaseWindow.ExternalId, false, It.IsAny<bool>())).Returns(releaseWindow);
            _releaseApproverGatewayMock.Setup(r => r.GetApprovers(releaseWindow.ExternalId))
                .Returns(new List<ReleaseApprover>());

            Sut.Handle(command);

            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ApproverRemovedFromReleaseEvent>(
                            a => a.ApproverId == command.ApproverId && a.ReleaseWindowId == releaseWindow.ExternalId)));
            _releaseWindowGatewayMock.Verify(r => r.ApproveRelease(releaseWindow.ExternalId), Times.Never);
        }

        [Test]
        public void Handle_ShouldApproveRelease_WhenApproverRemovedAndReleaseIsFullyApproved()
        {
            var command = new RemoveReleaseApproverCommand
            {
                ApproverId = Guid.NewGuid(),
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext()
            };
            var releaseWindow = new ReleaseWindow { ExternalId = command.ReleaseWindowId, ReleaseType = ReleaseType.Scheduled };
            _releaseWindowGatewayMock.Setup(r => r.GetByExternalId(releaseWindow.ExternalId, false, It.IsAny<bool>())).Returns(releaseWindow);

            _businessRuleEngineMock.Setup(o => o.Execute<bool>(Guid.Empty,
                    BusinessRuleConstants.Release.ReleaseApprovalRule.ExternalId, It.IsAny<IDictionary<string, object>>()))
                .Returns(true);
            _productGatewayMock.Setup(x => x.GetProducts(releaseWindow.ExternalId))
                .Returns(new[] { new Product { ReleaseTrack = ReleaseTrack.Manual } });

            Sut.Handle(command);

            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ApproverRemovedFromReleaseEvent>(
                            a => a.ApproverId == command.ApproverId && a.ReleaseWindowId == releaseWindow.ExternalId)));
            _releaseWindowGatewayMock.Verify(r => r.ApproveRelease(releaseWindow.ExternalId));
        }

        [Test]
        public void Handle_ShouldPublishApproveReleaseEvent_WhenApproverRemovedAndReleaseIsFullyApproved()
        {
            var command = new RemoveReleaseApproverCommand
            {
                ApproverId = Guid.NewGuid(),
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext()
            };
            var releaseWindow = new ReleaseWindow { ExternalId = command.ReleaseWindowId, ReleaseType = ReleaseType.Scheduled };
            _releaseWindowGatewayMock.Setup(r => r.GetByExternalId(releaseWindow.ExternalId, false, It.IsAny<bool>())).Returns(releaseWindow);

            _businessRuleEngineMock.Setup(o => o.Execute<bool>(Guid.Empty,
                    BusinessRuleConstants.Release.ReleaseApprovalRule.ExternalId,
                    It.Is<IDictionary<string, object>>(x => x["releaseWindow"] == releaseWindow)))
                .Returns(true);
            _productGatewayMock.Setup(x => x.GetProducts(releaseWindow.ExternalId))
                .Returns(new[] { new Product { ReleaseTrack = ReleaseTrack.Manual } });

            Sut.Handle(command);

            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseWindowApprovedEvent>(
                            a => a.ReleaseWindow == releaseWindow)));
            _eventPublisherMock.Verify(
                o =>
                    o.Publish(
                        It.Is<ReleaseStatusChangedEvent>(
                            evnt =>
                                evnt.ReleaseWindowId == releaseWindow.ExternalId &&
                                evnt.ReleaseStatus == EnumDescriptionHelper.GetDescription(ReleaseStatus.Approved))));
        }

        [Test]
        public void Handle_ShouldChangeDecisionToGo_WhenProductIsOnManualTrack()
        {
            var command = new RemoveReleaseApproverCommand
            {
                ApproverId = Guid.NewGuid(),
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext()
            };
            var releaseWindow = new ReleaseWindow { ExternalId = command.ReleaseWindowId, ReleaseType = ReleaseType.Scheduled };
            _releaseWindowGatewayMock.Setup(r => r.GetByExternalId(releaseWindow.ExternalId, false, It.IsAny<bool>())).Returns(releaseWindow);

            _businessRuleEngineMock.Setup(o => o.Execute<bool>(Guid.Empty,
                    BusinessRuleConstants.Release.ReleaseApprovalRule.ExternalId, It.IsAny<IDictionary<string, object>>()))
                .Returns(true);
            _productGatewayMock.Setup(x => x.GetProducts(releaseWindow.ExternalId))
                .Returns(new[] { new Product { ReleaseTrack = ReleaseTrack.Manual } });

            Sut.Handle(command);

            _commandDispatcherMock.Verify(x => x.Send(It.Is<UpdateReleaseDecisionCommand>(
                c => c.ReleaseDecision == ReleaseDecision.Go
                      && c.ReleaseWindowId == command.ReleaseWindowId)), Times.Once);
        }

        [Test]
        public void Handle_ShouldChangeDecisionToNoGo_WhenProductIsOnManualTrackAndNotAllChecksPassed()
        {
            _getStatusQueryMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()))
                .Returns(new GetContinuousDeliveryStatusResponse
                {
                    StatusCheck = new List<StatusCheckItem>
                    {
                        new StatusCheckItem
                        {
                            MetricControl = "test",
                            Status = StatusType.Green
                        },
                        new StatusCheckItem
                        {
                            MetricControl = "a",
                            Status = StatusType.Yellow
                        },
                        new StatusCheckItem
                        {
                            MetricControl = "b",
                            Status = StatusType.Green
                        }
                    }
                });

            var command = new RemoveReleaseApproverCommand
            {
                ApproverId = Guid.NewGuid(),
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext()
            };
            var releaseWindow = new ReleaseWindow { ExternalId = command.ReleaseWindowId, ReleaseType = ReleaseType.Scheduled };
            _releaseWindowGatewayMock.Setup(r => r.GetByExternalId(releaseWindow.ExternalId, false, It.IsAny<bool>())).Returns(releaseWindow);

            _businessRuleEngineMock.Setup(o => o.Execute<bool>(Guid.Empty,
                    BusinessRuleConstants.Release.ReleaseApprovalRule.ExternalId, It.IsAny<IDictionary<string, object>>()))
                .Returns(true);
            _productGatewayMock.Setup(x => x.GetProducts(releaseWindow.ExternalId))
                .Returns(new[] { new Product { ReleaseTrack = ReleaseTrack.Manual } });

            Sut.Handle(command);

            _commandDispatcherMock.Verify(x => x.Send(It.Is<UpdateReleaseDecisionCommand>(
                c => c.ReleaseDecision == ReleaseDecision.NoGo
                      && c.ReleaseWindowId == command.ReleaseWindowId)), Times.Once);
        }

        [Test]
        public void Handle_ShouldNotChangeDecision_WhenProductIsNotOnManualTrack()
        {
            var command = new RemoveReleaseApproverCommand
            {
                ApproverId = Guid.NewGuid(),
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext()
            };
            var releaseWindow = new ReleaseWindow { ExternalId = command.ReleaseWindowId, ReleaseType = ReleaseType.Scheduled };
            _releaseWindowGatewayMock.Setup(r => r.GetByExternalId(releaseWindow.ExternalId, false, It.IsAny<bool>())).Returns(releaseWindow);

            _businessRuleEngineMock.Setup(o => o.Execute<bool>(Guid.Empty,
                    BusinessRuleConstants.Release.ReleaseApprovalRule.ExternalId, It.IsAny<IDictionary<string, object>>()))
                .Returns(true);

            _productGatewayMock.Setup(x => x.GetProducts(releaseWindow.ExternalId))
                .Returns(new[] { new Product { ReleaseTrack = ReleaseTrack.PreApproved } });

            Sut.Handle(command);

            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<UpdateReleaseDecisionCommand>()), Times.Never);
        }

        [Test]
        [ExpectedException(typeof(FailedToAuthenticateException))]
        public void Handle_ShouldThrowFaildToAuthenticateException_WhenCreadentialsAreWrong()
        {
            var command = new ApproveReleaseCommand
            {
                AccountId = Guid.NewGuid(),
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext(),
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10)
            };

            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns((Session)null);

            Sut.Handle(command);
        }


        [Test]
        public void Handle_ApproveShouldApproveAndPublishEventWithCredentialId_WhenCommandAccountIdAndCredentialIdAreDifferent()
        {
            var accountId = Guid.NewGuid();
            var credentialsAccountId = Guid.NewGuid();
            var releaseWindowId = Guid.NewGuid();
            var command = new ApproveReleaseCommand
            {
                AccountId = accountId,
                ReleaseWindowId = releaseWindowId,
                CommandContext = new CommandContext(),
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10),
                Comment = "super desc"
            };

            _accountsBusinessLogicMock.Setup(o => o.SignSession(command.UserName, command.Password))
                .Returns(new Session { AccountId = credentialsAccountId });

            _releaseWindowGatewayMock.Setup(o => o.GetByExternalId(releaseWindowId, It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(Builder<ReleaseWindow>.CreateNew()
                    .With(o => o.ExternalId, releaseWindowId)
                    .With(o => o.ReleaseType, ReleaseType.Scheduled)
                    .Build());
            _accountsGatewayMock.Setup(o => o.GetAccount(credentialsAccountId, true))
                .Returns(new Account { ExternalId = credentialsAccountId });

            var approvers = new[]
            {
                new ReleaseApprover
                {
                    Account = new Account {Role = new Role {Name = "ProductOwner"}, ExternalId = accountId},
                    ApprovedOn = DateTime.Now,
                    ExternalId = Guid.NewGuid()
                },
                new ReleaseApprover
                {
                    Account = new Account {Role = new Role {Name = "ReleaseEngineer"}, ExternalId = credentialsAccountId},
                    ApprovedOn = DateTime.Now,
                    ExternalId = Guid.NewGuid()
                }
            };

            _releaseApproverGatewayMock.Setup(o => o.GetApprovers(releaseWindowId)).Returns(approvers);

            Sut.Handle(command);

            _releaseApproverGatewayMock.Verify(o => o.ApproveRelease(credentialsAccountId, releaseWindowId, "super desc"));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ApprovementEvent>(
                            a => a.Comment == "super desc"
                                && a.ReleaseWindowId == releaseWindowId
                                && a.ApproverId == credentialsAccountId)));
        }

    }
}
