using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.EventHandlers.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;
using System;
using System.Collections.Generic;
using ReMi.Common.Utils.Enums;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.Email;

namespace ReMi.EventHandlers.Tests.ReleaseCalendar
{
    public class UpdateReleaseShouldNotifyTeamMembersEventHandlerTests
        : TestClassFor<UpdateWindowShouldNotifyTeamMembersEventHandler>
    {
        private Mock<IAccountNotificationGateway> _accountNotificationGatewayMock;
        private Mock<IReleaseParticipantGateway> _releaseParticipantGatewayMock;
        private Mock<IEmailTextProvider> _emailTextProviderMock;
        private Mock<IEmailService> _emailClientMock;
        private const String WindowUpdatedSubject = "Window Was updated";

        protected override UpdateWindowShouldNotifyTeamMembersEventHandler ConstructSystemUnderTest()
        {
            return new UpdateWindowShouldNotifyTeamMembersEventHandler
            {
                EmailService = _emailClientMock.Object,
                EmailTextProvider = _emailTextProviderMock.Object,
                AccountNotificationGatewayFactory = () => _accountNotificationGatewayMock.Object,
                ReleaseParticipantGatewayFactory = () => _releaseParticipantGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountNotificationGatewayMock = new Mock<IAccountNotificationGateway>();
            _emailClientMock = new Mock<IEmailService>();
            _emailTextProviderMock = new Mock<IEmailTextProvider>();
            _releaseParticipantGatewayMock = new Mock<IReleaseParticipantGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldSendCorrectNotificationEmailToTeamMembers()
        {
            var evnt = new ReleaseWindowUpdatedEvent
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
            var account = new Account
            {
                Email = RandomData.RandomEmail(),
                FullName = RandomData.RandomString(22),
                Role = new Role { Name = "TeamMember" },
                ExternalId = Guid.NewGuid()
            };

            var participant = new Account
            {
                Email = RandomData.RandomEmail(),
                FullName = RandomData.RandomString(14),
                Role = new Role { Name = "TeamMember" },
                ExternalId = Guid.NewGuid()
            };

            _accountNotificationGatewayMock.Setup(
                a => a.GetSubscribers(NotificationType.ReleaseWindowsSchedule, evnt.ReleaseWindow.Products))
                .Returns(new List<Account> { account });

            _releaseParticipantGatewayMock.Setup(x => x.GetReleaseParticipants(evnt.ReleaseWindow.ExternalId))
                .Returns(new List<ReleaseParticipant>
                {
                    new ReleaseParticipant
                    {
                        Account = participant
                    }
                });

            var mail = RandomData.RandomString(200);
            _emailTextProviderMock.Setup(
                e => e.GetText("ReleaseWindowUpdatedEmail", It.IsAny<Dictionary<String, object>>()))
                .Returns(mail);

            Sut.Handle(evnt);

            _accountNotificationGatewayMock.Verify(
                a => a.GetSubscribers(NotificationType.ReleaseWindowsSchedule, evnt.ReleaseWindow.Products));

            _releaseParticipantGatewayMock.Verify(x => x.GetReleaseParticipants(evnt.ReleaseWindow.ExternalId));

            _emailTextProviderMock.Verify(
                e => e.GetText("ReleaseWindowUpdatedEmail", It.IsAny<Dictionary<String, object>>()));

            _emailClientMock.Verify(
                e =>
                    e.Send(account.Email,
                        String.Format("{0} {1} {2}", evnt.ReleaseWindow.Products.FormatElements(string.Empty, string.Empty),
                            EnumDescriptionHelper.GetDescription(evnt.ReleaseWindow.ReleaseType), WindowUpdatedSubject),
                        mail));

        }
    }
}
