using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.EventHandlers.ReleasePlan;
using ReMi.Events.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Contracts.Plugins.Services.Email;

namespace ReMi.EventHandlers.Tests.ReleasePlan
{
    public class ReleaseParticipationHandlerTests : TestClassFor<ReleaseParticipationHandler>
    {
        private readonly Mock<IEmailService> _emailClient = new Mock<IEmailService>();
        private readonly Mock<IEmailTextProvider> _emailTextProviderMock = new Mock<IEmailTextProvider>(MockBehavior.Strict);
        private readonly Mock<IReleaseWindowGateway> _releaseWindowGateway = new Mock<IReleaseWindowGateway>();
        private readonly Mock<IApplicationSettings> _applicationSettingsMock = new Mock<IApplicationSettings>();

        private readonly Account _account = new Account
        {
            FullName = RandomData.RandomString(10),
            Email = RandomData.RandomEmail(),
            Role = new Role { Name = "ProductOwner" },
            ExternalId = Guid.NewGuid()
        };

        private ReleaseParticipant _releaseParticipant;

        private readonly ReleaseWindow _releaseWindow = new ReleaseWindow
        {
            StartTime = SystemTime.Now,
            ReleaseType = ReleaseType.Scheduled,
            Products = new[] { RandomData.RandomString(1, 10) },
            Sprint = RandomData.RandomString(1, 50),
            ExternalId = Guid.NewGuid()
        };

        protected override ReleaseParticipationHandler ConstructSystemUnderTest()
        {
            return new ReleaseParticipationHandler
            {
                EmailService = _emailClient.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGateway.Object,
                EmailTextProvider = _emailTextProviderMock.Object,
                ApplicationSettings = _applicationSettingsMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseParticipant = new ReleaseParticipant
            {
                Account = _account,
                ReleaseWindowId = _releaseWindow.ExternalId,
                ReleaseParticipantId = Guid.NewGuid()
            };

            _emailClient.Setup(email => email.Send(It.IsAny<List<string>>(), 
                string.Join(", ", _releaseWindow.Products) + " Release support team", It.IsAny<string>()));
            _releaseWindowGateway.Setup(rwg => rwg.GetByExternalId(_releaseWindow.ExternalId, false, false))
                .Returns(_releaseWindow);
            _emailClient.Setup(
                email =>
                    email.SendWithCalendarEvent(It.IsAny<string>(),
                    string.Join(", ", _releaseWindow.Products) + " Release Support Team", It.IsAny<string>(),
                        It.IsAny<CalendarEvent>(), It.IsAny<int>()));

            _applicationSettingsMock.SetupGet(o => o.DefaultReleaseWindowDurationTime)
                .Returns(RandomData.RandomInt(1, 100));

            base.TestInitialize();
        }

        [Test]
        public void SendNewReleaseParticipationTest()
        {
            _emailTextProviderMock.Setup(o => o.GetText("ParticipantAddedToReleaseWindowEmail", It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
                .Returns(string.Empty);

            Sut.Handle(new ReleaseParticipantsAddedEvent
            {
                Participants = new List<ReleaseParticipant> { _releaseParticipant },
                ReleaseWindowId = _releaseWindow.ExternalId
            });

            _releaseWindowGateway.Verify(rwg => rwg.GetByExternalId(_releaseParticipant.ReleaseWindowId, false, It.IsAny<bool>()));

            _emailClient.Verify(email =>
                email.SendWithCalendarEvent(It.IsAny<string>(), 
                string.Join(", ", _releaseWindow.Products) + " Release Support Team", It.IsAny<string>(),
                    It.IsAny<CalendarEvent>(), It.IsAny<int>()));

            _emailTextProviderMock.Verify(o => o.GetText("ParticipantAddedToReleaseWindowEmail",
                It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "Participant" && _releaseParticipant.Account.FullName.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Sprint" && _releaseWindow.Sprint.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Products" && string.Join(", ", _releaseWindow.Products).Equals(pair.Value))
                    && x.Any(pair => pair.Key == "StartTime"
                        && _releaseWindow.StartTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm").Equals(pair.Value.ToString()))
                    && x.Any(pair => pair.Key == "AcknowledgeUrl")
                )));
        }

        [Test]
        [ExpectedException(typeof(ReleaseNotFoundException))]
        public void SendNewReleaseParticipation_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            Sut.Handle(new ReleaseParticipantsAddedEvent
            {
                Participants = new List<ReleaseParticipant> { _releaseParticipant },
                ReleaseWindowId = Guid.NewGuid()
            });
        }

        [Test]
        public void SendRemoveReleaseParticipationNotification_ShouldGenerateEmail_WhenInvoked()
        {
            _emailTextProviderMock.Setup(o => o.GetText("ParticipantRemovedFromReleaseWindowEmail", It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
                .Returns(string.Empty);

            Sut.Handle(new ReleaseParticipantRemovedEvent
            {
                Participant = _releaseParticipant,
                ReleaseWindowId = _releaseWindow.ExternalId
            });

            _releaseWindowGateway.Verify(rwg => rwg.GetByExternalId(_releaseWindow.ExternalId, false, false));
            _emailClient.Verify(email =>
                email.SendWithCalendarEvent(It.IsAny<string>(),
                string.Join(", ", _releaseWindow.Products) + " Release Support Team", It.IsAny<string>(),
                    It.IsAny<CalendarEvent>(), It.IsAny<int>()));

            _emailTextProviderMock.Verify(o => o.GetText("ParticipantRemovedFromReleaseWindowEmail",
                It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "Participant" && _releaseParticipant.Account.FullName.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Sprint" && _releaseWindow.Sprint.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Products" && string.Join(", ", _releaseWindow.Products).Equals(pair.Value))
                    && x.Any(pair => pair.Key == "StartTime")
                    && x.Any(pair => pair.Key == "StartTime"
                        && _releaseWindow.StartTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm").Equals(pair.Value.ToString()))
                    && x.Any(pair => pair.Key == "ReleasePlanUrl")
                )));
        }

        [Test]
        public void SendRemoveReleaseParticipationNotification_ShouldSendEmail_WhenInvoked()
        {
            _emailTextProviderMock.Setup(o => o.GetText("ParticipantRemovedFromReleaseWindowEmail", It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
                .Returns(string.Empty);


            Sut.Handle(new ReleaseParticipantRemovedEvent
            {
                Participant = _releaseParticipant,
                ReleaseWindowId = _releaseWindow.ExternalId
            });

            _emailClient.Verify(email =>
                email.SendWithCalendarEvent(It.IsAny<string>(),
                string.Join(", ", _releaseWindow.Products) + " Release Support Team", It.IsAny<string>(),
                    It.IsAny<CalendarEvent>(), It.IsAny<int>()));

            _emailTextProviderMock.Verify(o => o.GetText("ParticipantRemovedFromReleaseWindowEmail",
                It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "Participant" && _releaseParticipant.Account.FullName.Equals(pair.Value)) //
                    && x.Any(pair => pair.Key == "Sprint" && _releaseWindow.Sprint.Equals(pair.Value)) //
                    && x.Any(pair => pair.Key == "Products" && string.Join(", ", _releaseWindow.Products).Equals(pair.Value))
                    && x.Any(pair => pair.Key == "StartTime") // && _releaseWindow.StartTime.Equals(pair.Value)
                    && x.Any(pair => pair.Key == "ReleasePlanUrl")
                )));
        }
    }
}
