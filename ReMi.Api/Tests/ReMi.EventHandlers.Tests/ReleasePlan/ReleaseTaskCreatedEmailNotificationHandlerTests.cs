using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.EventHandlers.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.EventHandlers.Tests.ReleasePlan
{
    public class ReleaseTaskCreatedEmailNotificationHandlerTests
        : TestClassFor<ReleaseTaskCreatedEmailNotificationHandler>
    {
        private Mock<IAccountsGateway> _accountGatewayMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IEmailService> _emailClientMock;
        private Mock<IEmailTextProvider> _emailTextProviderMock;
        private Mock<IAccountNotificationGateway> _accountNotificationGatewayMock;
        private List<Account> _members;
        private Guid _windowId;

        protected override ReleaseTaskCreatedEmailNotificationHandler ConstructSystemUnderTest()
        {
            return new ReleaseTaskCreatedEmailNotificationHandler
            {
                AccountsGatewayFactory = () => _accountGatewayMock.Object,
                EmailService = _emailClientMock.Object,
                EmailTextProvider = _emailTextProviderMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                AccountNotificationGatewayFactory = () => _accountNotificationGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountGatewayMock = new Mock<IAccountsGateway>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _emailTextProviderMock = new Mock<IEmailTextProvider>();
            _emailClientMock = new Mock<IEmailService>();
            _accountNotificationGatewayMock = new Mock<IAccountNotificationGateway>();

            _members = new List<Account>
            {
                new Account
                {
                    FullName = RandomData.RandomString(10, 12),
                    Email = RandomData.RandomEmail(),
                    ExternalId = Guid.NewGuid(),
                    Name = RandomData.RandomString(5, 8)
                }
            };
            _windowId = Guid.NewGuid();

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Handle_ShouldThrowException_WhenEventNotInitialized()
        {
            Sut.Handle(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Handle_ShouldThrowException_WhenReleaseTaskNotInitialized()
        {
            Sut.Handle(new ReleaseTaskCreatedEvent());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Handle_ShouldThrowException_WhenAssigneeExternalIdIsMissing()
        {
            Sut.Handle(new ReleaseTaskCreatedEvent { ReleaseTask = new ReleaseTask { CreatedByExternalId = null } });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Handle_ShouldThrowException_WhenAuthorExternalIdIsMissing()
        {
            Sut.Handle(new ReleaseTaskCreatedEvent { ReleaseTask = new ReleaseTask() });
        }

        [Test]
        public void Handle_ShouldGenerateEmailTemplateForAssignee_WhenInvoked()
        {
            var account = SetupAccount();
            var releaseWindow = SetupReleaseWindow();

            _accountNotificationGatewayMock.Setup(
                x => x.GetSubscribers(NotificationType.ReleaseTasks, releaseWindow.Products)).Returns(_members);

            var releaseTask = Builder<ReleaseTask>.CreateNew()
                .With(o => o.ReleaseWindowId, releaseWindow.ExternalId)
                .With(o => o.CreatedByExternalId, account.ExternalId)
                .With(o => o.AssigneeExternalId, account.ExternalId)
                .Build();

            Sut.Handle(new ReleaseTaskCreatedEvent { ReleaseTask = releaseTask });

            _accountNotificationGatewayMock.Verify(
               x => x.GetSubscribers(NotificationType.ReleaseTasks, releaseWindow.Products));
            _emailTextProviderMock.Verify(e => e.GetText("TaskWasAssignedEmail",
                It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "Description" && pair.Value.ToString() == releaseTask.Description)
                    &&
                    x.Any(
                        pair =>
                            pair.Key == "Type" &&
                            pair.Value.ToString() == releaseTask.Type.ToString(CultureInfo.InvariantCulture))
                    &&
                    x.Any(
                        pair =>
                            pair.Key == "Risk" &&
                            pair.Value.ToString() == releaseTask.Risk.ToString(CultureInfo.InvariantCulture))
                    && x.Any(pair => pair.Key == "HelpDeskUrl" && pair.Value.ToString() == "not created")
                    && x.Any(pair => pair.Key == "Products" && pair.Value.ToString() == string.Join(", ", releaseWindow.Products))
                    && x.Any(pair => pair.Key == "Sprint" && pair.Value.ToString() == releaseWindow.Sprint)
                    && x.Any(pair => pair.Key == "Creator" && pair.Value.ToString() == releaseTask.CreatedBy)
                    && x.Any(pair => pair.Key == "Assignee" && pair.Value.ToString() == account.FullName)
                    &&
                    x.Any(
                        pair =>
                            pair.Key == "StartTime" &&
                            pair.Value.ToString() ==
                            String.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime()))
                    && x.Any(pair => pair.Key == "ConfirmUrl")
                    )));
        }

        [Test]
        public void Handle_ShouldSendEmailToAssignee()
        {
            var account = SetupAccount();
            var releaseWindow = SetupReleaseWindow();
            var releaseTask = Builder<ReleaseTask>.CreateNew()
                .With(o => o.ReleaseWindowId, releaseWindow.ExternalId)
                .With(o => o.AssigneeExternalId, account.ExternalId)
                .With(o => o.CreatedByExternalId, account.ExternalId)
                .Build();

            _accountNotificationGatewayMock.Setup(
               x => x.GetSubscribers(NotificationType.ReleaseTasks, releaseWindow.Products)).Returns(_members);

            Sut.Handle(new ReleaseTaskCreatedEvent { ReleaseTask = releaseTask });

            _accountNotificationGatewayMock.Verify(
               x => x.GetSubscribers(NotificationType.ReleaseTasks, releaseWindow.Products));
            _emailClientMock.Verify(e => e.Send(account.Email,
                string.Join(", ", releaseWindow.Products) + " Release task created", It.IsAny<string>()));
        }

        [Test]
        public void Handle_ShouldSendEmailToAssigneeAndSubscriner_WhenAssigneeAndSubscriberAreNotOnePerson()
        {
            var creator = SetupAccount("creator@email.com");
            var assignee = SetupAccount("assignee@email.com");
            var releaseWindow = SetupReleaseWindow();
            var releaseTask =
                Builder<ReleaseTask>.CreateNew()
                .With(o => o.ReleaseWindowId, releaseWindow.ExternalId)
                    .With(o => o.AssigneeExternalId, assignee.ExternalId)
                    .With(o => o.CreatedByExternalId, creator.ExternalId)
                    .Build();

            _accountNotificationGatewayMock.Setup(
                x => x.GetSubscribers(NotificationType.ReleaseTasks, releaseWindow.Products))
                .Returns(new List<Account> { creator });

            Sut.Handle(new ReleaseTaskCreatedEvent { ReleaseTask = releaseTask });

            _accountGatewayMock.Verify(acc => acc.GetAccount(releaseTask.AssigneeExternalId, true));

            _emailClientMock.Verify(e => e.Send(creator.Email,
                string.Join(", ", releaseWindow.Products) + " Release task created", It.IsAny<string>()));
            _emailClientMock.Verify(e => e.Send(assignee.Email,
                string.Join(", ", releaseWindow.Products) + " Release task created", It.IsAny<string>()));
        }

        private ReleaseWindow SetupReleaseWindow(Guid? releaseWindowId = null)
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(o => o.ExternalId, releaseWindowId ?? _windowId)
                .With(o => o.Products, new[] { RandomData.RandomString(10) })
                .Build();

            _releaseWindowGatewayMock.Setup(o => o.GetByExternalId(It.Is<Guid>(x => x.Equals(releaseWindow.ExternalId)), It.Is<bool>(x => x), It.IsAny<bool>()))
                .Returns(releaseWindow);

            return releaseWindow;
        }

        public Account SetupAccount(string email = null)
        {
            var account = Builder<Account>.CreateNew()
                .With(o => o.Email, email ?? RandomData.RandomEmail())
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _accountGatewayMock.Setup(acc => acc.GetAccount(account.ExternalId, true)).Returns(account);
            _accountGatewayMock.Setup(acc => acc.GetAccountByEmail(account.Email)).Returns(account);

            return account;
        }
    }
}
