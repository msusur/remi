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
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.EventHandlers.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.Tests
{
    public class ReleaseWindowCancelledSendNotificationToPartticipantsHandlerTests : TestClassFor<ReleaseWindowCanceledSendNotificationHandler>
    {
        private Mock<IAccountNotificationGateway> _accountNotificationsGatewayMock;
        private Mock<IEmailService> _emailClientMock;
        private Mock<IEmailTextProvider> _emailTextProviderMock;
        private Mock<IApplicationSettings> _applicationSettingsMock;

        protected override ReleaseWindowCanceledSendNotificationHandler ConstructSystemUnderTest()
        {
            return new ReleaseWindowCanceledSendNotificationHandler
            {
                AccountNotificationGatewayFactory = () => _accountNotificationsGatewayMock.Object,
                EmailService = _emailClientMock.Object,
                EmailTextProvider = _emailTextProviderMock.Object,
                ApplicationSettings = _applicationSettingsMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountNotificationsGatewayMock = new Mock<IAccountNotificationGateway>();
            _applicationSettingsMock = new Mock<IApplicationSettings>();
            _emailClientMock = new Mock<IEmailService>();
            _emailTextProviderMock = new Mock<IEmailTextProvider>();

            base.TestInitialize();
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Handle_ShouldThrowsException_WhenWindowIsEmpty()
        {
            Sut.Handle(new ReleaseWindowCanceledEvent());
        }
       
        [Test]
        public void Handle_ShouldSendAllNotifications_WhenParticipantListAndReleaseWindowIsPresent()
        {
            var evnt = new ReleaseWindowCanceledEvent
            {
                ReleaseWindow = new ReleaseWindow
                {
                    ExternalId = Guid.NewGuid(),
                    Products = new[] { RandomData.RandomString(5) },
                    Sprint = RandomData.RandomString(14, 25),
                    StartTime = DateTime.UtcNow,
                    ReleaseType = ReleaseType.Scheduled
                },
                Participants = new List<Account>
                {
                    new Account
                    {
                        ExternalId = Guid.NewGuid(),
                        FullName = RandomData.RandomString(4, 18),
                        Email = RandomData.RandomEmail()
                    }
                }
            };

            _emailTextProviderMock.Setup(x => x.GetText("ReleaseWindowCancelled", It.IsAny<Dictionary<String, object>>()))
                .Returns("email");
            _applicationSettingsMock.Setup(x => x.DefaultReleaseWindowDurationTime).Returns(120);
            _accountNotificationsGatewayMock.Setup(
                a => a.GetSubscribers(NotificationType.ReleaseWindowsSchedule, evnt.ReleaseWindow.Products))
                .Returns(new List<Account>
                {
                    new Account
                    {
                        ExternalId = Guid.NewGuid(),
                        Email = "email@mail.com",
                        FullName = "subscriber"
                    }
                });

            Sut.Handle(evnt);

            _emailClientMock.Verify(
                x =>
                    x.SendWithCalendarEvent(
                        evnt.Participants.First().Email,
                        string.Join(", ", evnt.ReleaseWindow.Products) + " Release Support Team",
                        "email",
                        It.Is<CalendarEvent>(
                            o =>
                                o.AppointmentId == evnt.ReleaseWindow.ExternalId.ToString() &&
                                o.CalendarEventType == CalendarEventType.CANCEL &&
                                o.StartTime.Equals(evnt.ReleaseWindow.StartTime)), 120));

            _emailClientMock.Verify(
                x =>
                    x.Send(
                        "email@mail.com",
                        string.Join(", ", evnt.ReleaseWindow.Products) + " Release Support Team",
                        "email"));

            _accountNotificationsGatewayMock.Verify(
                a => a.GetSubscribers(NotificationType.ReleaseWindowsSchedule, evnt.ReleaseWindow.Products));

            _emailTextProviderMock.Verify(
                e => e.GetText("ReleaseWindowCancelled", It.Is<Dictionary<string, object>>(x =>
                    x.Any(pair => pair.Key == "Sprint" && evnt.ReleaseWindow.Sprint.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Products" && string.Join(", ", evnt.ReleaseWindow.Products).Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Assignee" && evnt.Participants.First().FullName.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "StartTime"
                                     &&
                                     evnt.ReleaseWindow.StartTime.ToLocalTime()
                                         .ToString("dd/MM/yyyy HH:mm")
                                         .Equals(pair.Value.ToString()))
                    && x.Any(pair => pair.Key == "ReleaseType" && pair.Value.Equals("Scheduled Release"))
                    && x.Any(pair => pair.Key == "ReleaseCalendarUrl")
                    )));

            _emailTextProviderMock.Verify(
                e => e.GetText("ReleaseWindowCancelled", It.Is<Dictionary<string, object>>(x =>
                    x.Any(pair => pair.Key == "Sprint" && evnt.ReleaseWindow.Sprint.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Products" && string.Join(", ", evnt.ReleaseWindow.Products).Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Assignee" && "subscriber".Equals(pair.Value))
                    && x.Any(pair => pair.Key == "StartTime"
                                     &&
                                     evnt.ReleaseWindow.StartTime.ToLocalTime()
                                         .ToString("dd/MM/yyyy HH:mm")
                                         .Equals(pair.Value.ToString()))
                    && x.Any(pair => pair.Key == "ReleaseType" && pair.Value.Equals("Scheduled Release"))
                    && x.Any(pair => pair.Key == "ReleaseCalendarUrl")
                    )));
        }
    }
}
