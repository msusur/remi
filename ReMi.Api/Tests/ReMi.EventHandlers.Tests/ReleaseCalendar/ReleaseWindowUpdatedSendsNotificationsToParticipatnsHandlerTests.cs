using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.EventHandlers.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.Tests.ReleaseCalendar
{
    public class ReleaseWindowUpdatedSendsNotificationsToParticipatnsHandlerTests :
        TestClassFor<ReleaseWindowUpdatedSendsNotificationsToParticipatnsHandler>
    {
        private readonly Mock<IAccountsGateway> _accountGatewayMock = new Mock<IAccountsGateway>();
        private readonly Mock<IEmailService> _emailClient = new Mock<IEmailService>();
        private readonly Mock<IReleaseWindowGateway> _releaseWindowGateway = new Mock<IReleaseWindowGateway>();
        private readonly Mock<IApplicationSettings> _applicationSettingsMock = new Mock<IApplicationSettings>();
        private readonly Mock<IEmailTextProvider> _emailTextProvider = new Mock<IEmailTextProvider>();

        private readonly Mock<IReleaseParticipantGateway> _releaseParticipantGateway =
            new Mock<IReleaseParticipantGateway>();

        private readonly Account _account = new Account
        {
            FullName = RandomData.RandomString(10),
            Email = RandomData.RandomEmail(),
            Role = new Role { Name = "ProductOwner" },
            ExternalId = Guid.NewGuid()
        };

        private List<Account> _accounts;
        private ReleaseParticipant _releaseParticipant;

        private readonly ReleaseWindow _releaseWindow = new ReleaseWindow
        {
            StartTime = SystemTime.Now,
            ReleaseType = ReleaseType.Scheduled,
            Products = new[] { RandomData.RandomString(1, 10) },
            Sprint = RandomData.RandomString(1, 50),
            ExternalId = Guid.NewGuid()
        };


        protected override ReleaseWindowUpdatedSendsNotificationsToParticipatnsHandler ConstructSystemUnderTest()
        {
            return new ReleaseWindowUpdatedSendsNotificationsToParticipatnsHandler
            {
                EmailService = _emailClient.Object,
                ApplicationSettings = _applicationSettingsMock.Object,
                ReleaseParticipantGatewayFactory = () => _releaseParticipantGateway.Object,
                EmailTextProvider = _emailTextProvider.Object
            };
        }

        protected override void TestInitialize()
        {
            _accounts = new List<Account> { _account };
            _releaseParticipant = new ReleaseParticipant
            {
                Account = _account,
                ReleaseWindowId = _releaseWindow.ExternalId,
                ReleaseParticipantId = Guid.NewGuid()
            };

            _accountGatewayMock.Setup(gateway => gateway.GetProductOwners(new[] {"team"})).Returns(_accounts);
            _emailClient.Setup(email => email.Send(It.IsAny<List<string>>(), "Release support team", It.IsAny<string>()));
            _releaseWindowGateway.Setup(rwg => rwg.GetByExternalId(_releaseParticipant.ReleaseWindowId, It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(_releaseWindow);
            _releaseParticipantGateway.Setup(rp => rp.GetReleaseParticipants(_releaseWindow.ExternalId))
                .Returns(new List<ReleaseParticipant> { _releaseParticipant });
            _emailClient.Setup(
                email =>
                    email.SendWithCalendarEvent(It.IsAny<string>(), "Release Support Team", It.IsAny<string>(),
                        It.IsAny<CalendarEvent>(), It.IsAny<int>()));

            _applicationSettingsMock.SetupGet(o => o.DefaultReleaseWindowDurationTime)
                .Returns(RandomData.RandomInt(1, 100));

            _emailTextProvider.Setup(
               e => e.GetText("ReleaseWindowUpdatedParticipantEmail", It.IsAny<IEnumerable<KeyValuePair<string, object>>>())).Returns("mail");

            base.TestInitialize();
        }

        [Test]
        public void SendToParticipantsNotificationAboutUpdate_ShouldSendEmail_WhenInvoked()
        {
            var evnt = new ReleaseWindowUpdatedEvent
            {
                ReleaseWindow = _releaseWindow,
                Context = new EventContext
                {
                    UserId = _account.ExternalId
                }
            };

            Sut.Handle(evnt);

            _releaseParticipantGateway.Verify(rp => rp.GetReleaseParticipants(_releaseWindow.ExternalId));
            _emailClient.Verify(email =>
                email.SendWithCalendarEvent(It.IsAny<string>(),
                string.Join(", ", _releaseWindow.Products) + " Release Support Team", "mail",
                    It.IsAny<CalendarEvent>(), It.IsAny<int>()));
            _emailTextProvider.Verify(
                e => e.GetText("ReleaseWindowUpdatedParticipantEmail", It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "Assignee" && pair.Value.ToString().Equals(_releaseParticipant.Account.FullName))
                    && x.Any(pair => pair.Key == "Products" && pair.Value.ToString().Equals(string.Join(", ", _releaseWindow.Products)))
                    && x.Any(pair => pair.Key == "Sprint" && pair.Value.ToString().Equals(_releaseWindow.Sprint))
                    && x.Any(pair => pair.Key == "StartTime" && pair.Value.ToString().Equals(String.Format("{0:dd/MM/yyyy HH:mm}", _releaseWindow.StartTime)))
                    && x.Any(pair => pair.Key == "ConfirmationUrl"))));
        }

        [Test]
        public void SendToParticipantsNotificationAboutUpdateTest_ForProductOwner()
        {
            var evnt = new ReleaseWindowUpdatedEvent
            {
                ReleaseWindow = _releaseWindow,
                Context = new EventContext
                {
                    UserId = _account.ExternalId
                }
            };

            Sut.Handle(evnt);

            _releaseParticipantGateway.Verify(x => x.GetReleaseParticipants(_releaseWindow.ExternalId));
            _emailClient.Verify(email =>
                email.SendWithCalendarEvent(It.IsAny<string>(),
                string.Join(", ", _releaseWindow.Products) + " Release Support Team", "mail",
                    It.IsAny<CalendarEvent>(), It.IsAny<int>()));
        }

        [Test]
        public void SendToParticipantsNotificationAboutUpdateTest_ForBasicUser()
        {
            _releaseParticipantGateway.Setup(rp => rp.GetReleaseParticipants(_releaseWindow.ExternalId))
                .Returns(new List<ReleaseParticipant>
                {
                    new ReleaseParticipant
                    {
                        Account = new Account { Email = _account.Email, FullName = _account.FullName, Role = new Role { Name = "BasicUser" } }
                    }
                });

            var evnt = new ReleaseWindowUpdatedEvent
            {
                ReleaseWindow = _releaseWindow,
                Context = new EventContext
                {
                    UserId = _account.ExternalId
                }
            };

            Sut.Handle(evnt);

            _releaseParticipantGateway.Verify(x => x.GetReleaseParticipants(_releaseWindow.ExternalId));
            _emailClient.Verify(email =>
                email.SendWithCalendarEvent(It.IsAny<string>(),
                string.Join(", ", _releaseWindow.Products) + " Release Support Team", "mail",
                    It.IsAny<CalendarEvent>(), It.IsAny<int>()));
        }
    }
}
