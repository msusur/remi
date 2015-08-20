using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Commands.ReleaseExecution;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.Subscriptions;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.DataAccess.Exceptions;
using ReMi.EventHandlers.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;
using ReMi.Queries.ReleasePlan;

namespace ReMi.EventHandlers.Tests.ReleaseCalendar
{
    public class ReleaseWindowClosedEventHandlerTests : TestClassFor<ReleaseWindowClosedEventHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IProductGateway> _packageGatewayMock;
        private Mock<IAccountNotificationGateway> _accountNotificationGatewayMock;
        private Mock<IEmailService> _emailClientMock;
        private const string ReleaseClosedSubject = "Release closed";
        private List<Account> _addressees;
        private Account _addressee;
        private List<Account> _teamMembers;
        private Account _teamMember;
        private Guid _releaseWindowId;
        private ReleaseWindow _releaseWindow;
        private ReleaseWindowClosedEvent _event;
        private Mock<IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse>> _releaseContentQueryMock;
        private List<ReleaseContentTicket> _releaseContent;
        private Mock<IEmailTextProvider> _emailTextProviderMock;
        private string _email;
        private Mock<ICommandDispatcher> _commandDispatcherMock;

        protected override ReleaseWindowClosedEventHandler ConstructSystemUnderTest()
        {
            return new ReleaseWindowClosedEventHandler
            {
                EmailService = _emailClientMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                GetReleaseContentInformationQuery = _releaseContentQueryMock.Object,
                EmailTextProvider = _emailTextProviderMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object,
                AccountNotificationGatewayFactory = () => _accountNotificationGatewayMock.Object,
                PackageGatewayFactory = () => _packageGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountNotificationGatewayMock = new Mock<IAccountNotificationGateway>();
            _packageGatewayMock = new Mock<IProductGateway>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _emailClientMock = new Mock<IEmailService>();
            _releaseContentQueryMock = new Mock<IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse>>();
            _addressee = new Account { Email = RandomData.RandomEmail() };
            _addressees = new List<Account> { _addressee };
            _releaseWindowId = Guid.NewGuid();
            _email = RandomData.RandomString(100);
            _emailTextProviderMock = new Mock<IEmailTextProvider>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            _releaseWindow = new ReleaseWindow
            {
                ExternalId = _releaseWindowId,
                Sprint = RandomData.RandomString(4),
                Products = new[] { RandomData.RandomString(5) },
                ReleaseNotes = RandomData.RandomString(100),
                ReleaseType = ReleaseType.Scheduled
            };
            _event = new ReleaseWindowClosedEvent { ReleaseWindowId = _releaseWindowId, Recipients = _addressees };
            _teamMember = new Account { Email = RandomData.RandomEmail() };
            _teamMembers = new List<Account> { _teamMember };
            _releaseContent = new List<ReleaseContentTicket>
            {
                new ReleaseContentTicket { TicketDescription = RandomData.RandomString(50), IncludeToReleaseNotes = true },
                new ReleaseContentTicket { TicketDescription = RandomData.RandomString(20), IncludeToReleaseNotes = false }
            };
            _packageGatewayMock.Setup(x => x.GetProducts(_releaseWindowId))
                .Returns(new[] { new Product { ExternalId = Guid.NewGuid(), Description = _releaseWindow.Products.First() } });

            _releaseContentQueryMock.Setup(r => r.Handle(It.Is<GetReleaseContentInformationRequest>(x => x.ReleaseWindowId == _event.ReleaseWindowId)))
                .Returns(new GetReleaseContentInformationResponse { Content = _releaseContent });

            _releaseWindowGatewayMock.Setup(rwg => rwg.GetByExternalId(_releaseWindowId, false, It.IsAny<bool>())).Returns(_releaseWindow);
            _accountNotificationGatewayMock.Setup(
                rpg => rpg.GetSubscribers(NotificationType.Closing, _releaseWindow.Products))
                .Returns(_teamMembers);
            _emailTextProviderMock.Setup(
                e => e.GetText("ReleaseClosedEmail", It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
                .Returns(_email);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetReleaseWindow_WhenDataIsCorrect()
        {
            Sut.Handle(_event);

            _releaseWindowGatewayMock.Verify(rwg => rwg.GetByExternalId(_releaseWindowId, false, It.IsAny<bool>()));
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void Handle_ShouldThrowReleaseWindowNotFoundException_WhenReleaseWindowIsMissing()
        {
            Sut.Handle(new ReleaseWindowClosedEvent { Recipients = _addressees, ReleaseWindowId = Guid.NewGuid() });
        }

        [Test]
        public void Handle_ShouldGetReleaseParticipants_WhenDataIsCorrect()
        {
            Sut.Handle(_event);

            _accountNotificationGatewayMock.Verify(
                rpg => rpg.GetSubscribers(NotificationType.Closing, _releaseWindow.Products));
        }

        [Test]
        public void Handle_ShouldCallQuery_WhenDataIsCorrect()
        {
            Sut.Handle(_event);

            _releaseContentQueryMock.Verify(
                r =>
                    r.Handle(It.Is<GetReleaseContentInformationRequest>(x => x.ReleaseWindowId == _event.ReleaseWindowId)));
        }

        [Test]
        public void Handle_ShouldNotGetReleaseContent_WhenReleaseTypeIsNotScheduld()
        {
            _releaseWindow.ReleaseType = ReleaseType.Hotfix;

            Sut.Handle(_event);

            _releaseContentQueryMock.Verify(
                r =>
                    r.Handle(It.Is<GetReleaseContentInformationRequest>(x => x.ReleaseWindowId == _event.ReleaseWindowId)), Times.Never);
        }

        [Test]
        public void Handle_ShouldSendEmailNotification_WhenDataIsCorrect()
        {
            _releaseContent.First().TicketUrl = "tickeUrl";
            Sut.Handle(_event);

            _emailClientMock.Verify(rpg => rpg.Send(It.IsAny<List<string>>(),
                string.Join(", ", _releaseWindow.Products) + " " + ReleaseClosedSubject, _email));

            _emailTextProviderMock.Verify(o => o.GetText("ReleaseClosedEmail",
                It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "Sprint" && _releaseWindow.Sprint.Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Products" && string.Join(", ", _releaseWindow.Products).Equals(pair.Value))
                    && x.Any(pair => pair.Key == "Tickets" && pair.Value.ToString().Contains("tickeUrl"))
                    && x.Any(pair => pair.Key == "Notes")
                    && x.Any(pair => pair.Key == "ReleasePlanUrl")
                )));
        }

        [Test]
        public void Handle_ShouldSendUpdateTicketLabelsCommand()
        {
            Sut.Handle(_event);

            _packageGatewayMock.Verify(x => x.GetProducts(_releaseWindowId), Times.Once);
            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<UpdateTicketLabelsCommand>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldNotSendUpdateTicketLabelsCommand_WhenReleaseIsMarketAsFailed()
        {
            _event.IsFailed = true;

            Sut.Handle(_event);

            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<UpdateTicketLabelsCommand>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldCombineEventRecipientsAndSubscribersOnEmailSending_WhenInvoked()
        {
            Sut.Handle(_event);

            _emailClientMock.Verify(rpg =>
                rpg.Send(It.Is<List<string>>(x =>
                    x.Count == 2 && x.Contains(_teamMember.Email) && x.Contains(_addressee.Email)
                ),  string.Join(", ", _releaseWindow.Products) + " " + ReleaseClosedSubject, _email));
        }
    }
}
