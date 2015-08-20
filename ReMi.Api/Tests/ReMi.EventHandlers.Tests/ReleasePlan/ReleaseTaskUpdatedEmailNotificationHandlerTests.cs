using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.EventHandlers.ReleasePlan;
using ReMi.Events.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.Email;

namespace ReMi.EventHandlers.Tests.ReleasePlan
{
    public class ReleaseTaskUpdatedEmailNotificationHandlerTests : TestClassFor<ReleaseTaskUpdatedEmailNotificationHandler>
    {
        private Mock<IAccountsGateway> _accountGatewayMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IEmailService> _emailClientMock;
        private Mock<IEmailTextProvider> _emailTextProviderMock;
        private Mock<IAccountNotificationGateway> _accountNotificationGatewayMock;
        private List<Account> _members;
        private Guid _windowId;

        protected override ReleaseTaskUpdatedEmailNotificationHandler ConstructSystemUnderTest()
        {
            return new ReleaseTaskUpdatedEmailNotificationHandler
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
            Sut.Handle(new ReleaseTaskUpdatedEvent());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Handle_ShouldThrowException_WhenAssigneeExternalIdIsMissing()
        {
            Sut.Handle(new ReleaseTaskUpdatedEvent { ReleaseTask = new ReleaseTask { CreatedByExternalId = Guid.NewGuid() } });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Handle_ShouldThrowException_WhenAuthorExternalIdIsMissing()
        {
            Sut.Handle(new ReleaseTaskUpdatedEvent { ReleaseTask = new ReleaseTask() });
        }

        [Test]
        public void Handle_ShouldGenerateEmailTemplateForAssignee_WhenInvoked()
        {
            var releaseWindow = SetupReleaseWindow();
            var assignee = SetupAccount("assignee@email");
            var acc = SetupAccount(RandomData.RandomEmail());

            var releaseTask = Builder<ReleaseTask>.CreateNew()
                .With(o => o.ReleaseWindowId, releaseWindow.ExternalId)
                .With(o => o.AssigneeExternalId, assignee.ExternalId)
                .With(o => o.HelpDeskTicketReference, null)
                .Build();

            _emailTextProviderMock.Setup(
                e => e.GetText(It.IsAny<String>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
                .Returns("smth");

            _accountNotificationGatewayMock.Setup(
                x => x.GetSubscribers(NotificationType.ReleaseTasks, releaseWindow.Products))
                .Returns(new List<Account> { acc });

            Sut.Handle(new ReleaseTaskUpdatedEvent { ReleaseTask = releaseTask });

            _emailTextProviderMock.Verify(e => e.GetText("TaskWasUpdatedEmail",
                It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "Description" && pair.Value.ToString() == releaseTask.Description)
                    && x.Any(pair => pair.Key == "Type" && pair.Value.ToString() == releaseTask.Type.ToString(CultureInfo.InvariantCulture))
                    && x.Any(pair => pair.Key == "Risk" && pair.Value.ToString() == releaseTask.Risk.ToString(CultureInfo.InvariantCulture))
                    && x.Any(pair => pair.Key == "HelpDeskUrl" && pair.Value.ToString() == "not created")
                    && x.Any(pair => pair.Key == "Products" && pair.Value.ToString() == string.Join(", ", releaseWindow.Products))
                    && x.Any(pair => pair.Key == "Sprint" && pair.Value.ToString() == releaseWindow.Sprint)
                    && x.Any(pair => pair.Key == "Assignee" && pair.Value.ToString() == assignee.FullName)
                    && x.Any(pair => pair.Key == "StartTime" && pair.Value.ToString() == String.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime()))
                    && x.Any(pair => pair.Key == "ConfirmUrl")
                )));

            _emailTextProviderMock.Verify(e => e.GetText("TaskWasUpdatedSupportMembersEmail",
                It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "Recipient" && pair.Value.ToString() == acc.FullName)
                    &&
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
                    && x.Any(pair => pair.Key == "Assignee" && pair.Value.ToString() == assignee.FullName)
                    &&
                    x.Any(
                        pair =>
                            pair.Key == "StartTime" &&
                            pair.Value.ToString() ==
                            String.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime()))
                    && x.Any(pair => pair.Key == "ReleasePlanUrl")
                    )));

            _emailClientMock.Verify(e => e.Send(assignee.Email, 
                string.Join(", ", releaseWindow.Products) + " Release task updated", "smth"));
            _emailClientMock.Verify(e => e.Send(acc.Email,
                string.Join(", ", releaseWindow.Products) + " Release task updated", "smth"));
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
