using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.EventHandlers.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.Tests.ReleaseCalendar
{
    public class BookReleaseShouldNotifyTeamMembersEventHandlerTests
        : TestClassFor<ReleaseWindowBookedEventHandlerEmailNotification>
    {
        private Mock<IAccountNotificationGateway> _accountNotificationGatewayMock;
        private Mock<IEmailTextProvider> _emailTextProviderMock;
        private Mock<IEmailService> _emailClientMock;
        private const String WindowBookedSubject = "Window Was Booked";

        protected override ReleaseWindowBookedEventHandlerEmailNotification ConstructSystemUnderTest()
        {
            return new ReleaseWindowBookedEventHandlerEmailNotification
            {
                EmailService = _emailClientMock.Object,
                EmailTextProvider = _emailTextProviderMock.Object,
                AccountNotificationGatewayFactory = () => _accountNotificationGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountNotificationGatewayMock = new Mock<IAccountNotificationGateway>();
            _emailClientMock = new Mock<IEmailService>();
            _emailTextProviderMock = new Mock<IEmailTextProvider>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldNotSendEmail_WhenNoTeamMembers()
        {
            var evnt = new ReleaseWindowBookedEvent
            {
                ReleaseWindow =
                    new ReleaseWindow
                    {
                        Sprint = RandomData.RandomString(5),
                        StartTime = RandomData.RandomDateTime(),
                        Products = new[] { RandomData.RandomString(6) },
                        ReleaseType = ReleaseType.Scheduled
                    }
            };
            SetupAccounts(null);

            Sut.Handle(evnt);

            _emailClientMock.Verify(a => a.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldGetTeamMembers_WhenInvoked()
        {
            var evnt = new ReleaseWindowBookedEvent
            {
                ReleaseWindow =
                    new ReleaseWindow
                    {
                        Sprint = RandomData.RandomString(5),
                        StartTime = RandomData.RandomDateTime(),
                        Products = new[] { RandomData.RandomString(6) },
                        ReleaseType = ReleaseType.Scheduled

                    }
            };
            SetupAccounts(evnt.Products);

            _emailTextProviderMock.Setup(
                e => e.GetText("ReleaseWindowBookedEmail", It.IsAny<Dictionary<String, object>>()))
                .Returns(string.Empty);

            Sut.Handle(evnt);

            _accountNotificationGatewayMock.Verify(
                a => a.GetSubscribers(NotificationType.ReleaseWindowsSchedule, evnt.ReleaseWindow.Products));
        }

        [Test]
        public void Handle_ShouldSendEmail_WhenInvoked()
        {
            var evnt = new ReleaseWindowBookedEvent
            {
                ReleaseWindow =
                    new ReleaseWindow
                    {
                        Sprint = RandomData.RandomString(5),
                        StartTime = RandomData.RandomDateTime(),
                        Products = new[] { RandomData.RandomString(5) },
                        ReleaseType = ReleaseType.Scheduled
                    }
            };
            var account = SetupAccounts(evnt.Products, "ProductOwner");

            var email = RandomData.RandomString(200);
            _emailTextProviderMock.Setup(
                e => e.GetText("ReleaseWindowBookedEmail", It.IsAny<Dictionary<String, object>>()))
                .Returns(email);

            Sut.Handle(evnt);

            _emailTextProviderMock.Verify(e => e.GetText("ReleaseWindowBookedEmail", It.IsAny<Dictionary<String, object>>()));
            _emailClientMock.Verify(e => 
                e.Send(
                    account.Email, 
                    String.Format("{0} {1} {2}", 
                        evnt.ReleaseWindow.Products.FormatElements(string.Empty, string.Empty), 
                        EnumDescriptionHelper.GetDescription(evnt.ReleaseWindow.ReleaseType), 
                        WindowBookedSubject), 
                    email));

            _emailTextProviderMock.Verify(o => o.GetText("ReleaseWindowBookedEmail",
                It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "Assignee" && account.FullName.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Sprint" && evnt.ReleaseWindow.Sprint.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Products" && string.Join(", ", evnt.ReleaseWindow.Products).Equals(pair.Value))
                    && x.Any(pair => pair.Key == "StartTime" && evnt.ReleaseWindow.StartTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm").Equals(pair.Value.ToString()))
                    && x.Any(pair => pair.Key == "ReleasePlanUrl")
                )));
        }

        private Account SetupAccounts(IEnumerable<string> products, string role = "TeamMember")
        {
            if (!products.IsNullOrEmpty())
            {
                var account = new Account
                {
                    Email = RandomData.RandomEmail(),
                    FullName = RandomData.RandomString(50),
                    Role = new Role { Name = role },
                    ExternalId = Guid.NewGuid()
                };

                _accountNotificationGatewayMock.Setup(
                    a => a.GetSubscribers(NotificationType.ReleaseWindowsSchedule, products))
                    .Returns(new List<Account> { account });

                return account;
            }

            _accountNotificationGatewayMock.Setup(
                a => a.GetSubscribers(NotificationType.ReleaseWindowsSchedule, It.IsAny<IEnumerable<string>>()))
                .Returns(new List<Account>());

            return null;
        }
    }
}
