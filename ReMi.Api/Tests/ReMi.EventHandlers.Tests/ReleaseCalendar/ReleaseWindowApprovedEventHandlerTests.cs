using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.DataAccess.Exceptions;
using ReMi.EventHandlers.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;
using System;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.Email;

namespace ReMi.EventHandlers.Tests.ReleaseCalendar
{
    public class ReleaseWindowApprovedEventHandlerTests : TestClassFor<ReleaseWindowApprovedEventHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IReleaseApproverGateway> _approverGatewayMock;
        private Mock<IAccountNotificationGateway> _accountNotificationGatewayMock;
        private Mock<IEmailService> _emailClientMock;
        private Mock<IEmailTextProvider> _emailTextProviderMock; 

        protected override ReleaseWindowApprovedEventHandler ConstructSystemUnderTest()
        {
            return new ReleaseWindowApprovedEventHandler
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                AccountNotificationGatewayFactory = () => _accountNotificationGatewayMock.Object,
                ReleaseApproverGatewayFactory = () => _approverGatewayMock.Object,
                EmailService = _emailClientMock.Object,
                EmailTextProvider = _emailTextProviderMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _accountNotificationGatewayMock = new Mock<IAccountNotificationGateway>();
            _approverGatewayMock = new Mock<IReleaseApproverGateway>();
            _emailClientMock = new Mock<IEmailService>();
            _emailTextProviderMock = new Mock<IEmailTextProvider>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldNotifyAndSendCommand_WhenOccure()
        {
            var evnt = new ReleaseWindowApprovedEvent
            {
                ReleaseWindow =
                    new ReleaseWindow
                    {
                        ExternalId = Guid.NewGuid(),
                        Sprint = RandomData.RandomString(5, 20),
                        Products = new[] { RandomData.RandomString(5) },
                    }
            };

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(evnt.ReleaseWindow.ExternalId, false, false))
                .Returns(evnt.ReleaseWindow);
            _approverGatewayMock.Setup(x => x.GetApprovers(evnt.ReleaseWindow.ExternalId))
                .Returns(new List<ReleaseApprover>
                {
                    new ReleaseApprover
                    {
                        Comment = RandomData.RandomString(3, 75),
                        ApprovedOn = DateTime.UtcNow,
                        Account = new Account
                        {
                            FullName = RandomData.RandomString(14, 17)
                        }
                    }
                });
            _emailTextProviderMock.Setup(
                e => e.GetText("ReleaseWindowFullyApprovedEmail", It.IsAny<Dictionary<string, object>>()))
                .Returns("text");
            _accountNotificationGatewayMock.Setup(
                x => x.GetSubscribers(NotificationType.Approvement, evnt.ReleaseWindow.Products))
                .Returns(new List<Account>
                {
                    new Account
                    {
                        Email = "mail"
                    }
                });

            Sut.Handle(evnt);

            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(evnt.ReleaseWindow.ExternalId, false, false));
            _approverGatewayMock.Verify(x => x.GetApprovers(evnt.ReleaseWindow.ExternalId));
            _emailClientMock.Verify(
                x => x.Send(It.Is<IEnumerable<String>>(s => s.First() == "mail"), 
                    string.Join(", ", evnt.ReleaseWindow.Products) + " Release approved", "text"));
            _accountNotificationGatewayMock.Verify(
                x => x.GetSubscribers(NotificationType.Approvement, evnt.ReleaseWindow.Products));
            _emailTextProviderMock.Verify(
                e => e.GetText("ReleaseWindowFullyApprovedEmail", It.Is<Dictionary<string, object>>(x =>
                    x.Any(pair => pair.Key == "Sprint" && evnt.ReleaseWindow.Sprint.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Products" && string.Join(", ", evnt.ReleaseWindow.Products).Equals(pair.Value))
                    && x.Any(pair => pair.Key == "StartTime"
                                     &&
                                     evnt.ReleaseWindow.StartTime.ToLocalTime()
                                         .ToString("dd/MM/yyyy HH:mm")
                                         .Equals(pair.Value.ToString()))
                    && x.Any(pair => pair.Key == "ReleasePlanUrl"))));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Handle_ShouldThrowArgumentException_WhenReleaseWindowIsUndefined()
        {
            var evnt = new ReleaseWindowApprovedEvent();

            Sut.Handle(evnt);
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void Handle_ShouldThrowReleaseNotFoundException_WhenCannotFindWindow()
        {
            var evnt = new ReleaseWindowApprovedEvent{ReleaseWindow = new ReleaseWindow()};

            Sut.Handle(evnt);
        }
    }
}
