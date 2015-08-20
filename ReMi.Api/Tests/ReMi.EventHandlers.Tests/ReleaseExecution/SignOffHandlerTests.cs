using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.DataAccess.Exceptions;
using ReMi.EventHandlers.ReleaseExecution;
using ReMi.Events.ReleaseExecution;

namespace ReMi.EventHandlers.Tests.ReleaseExecution
{
    public class SignOffHandlerTests : TestClassFor<SignOffHandler>
    {
        private const String AddedToSignOffsSubject = " Added to sign offs";
        private const String RemovedFromSignOffsSubject = " Removed from signed offs";
        private const String ReleaseSignedOffSubject = " Release signed off";
        private Mock<IEmailTextProvider> _emailTextProviderMock;
        private Mock<IEmailService> _emailClientMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<ISignOffGateway> _signOffGatewayMock;
        private Mock<IAccountNotificationGateway> _accountNotificationGatewayMock;
        private String _email;
        private ReleaseWindow _window;
        private Account _account;
        private List<SignOff> _signOffs;
        private List<Account> _members; 

        protected override SignOffHandler ConstructSystemUnderTest()
        {
            return new SignOffHandler
            {
                EmailService = _emailClientMock.Object,
                EmailTextProvider = _emailTextProviderMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                SignOffGatewayFactory = () => _signOffGatewayMock.Object,
                AccountNotificationGatewayFactory = () => _accountNotificationGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _emailClientMock = new Mock<IEmailService>();
            _emailTextProviderMock = new Mock<IEmailTextProvider>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _signOffGatewayMock = new Mock<ISignOffGateway>();
            _accountNotificationGatewayMock = new Mock<IAccountNotificationGateway>();

            _email = RandomData.RandomString(100, 400);
            _emailTextProviderMock.Setup(e => e.GetText(It.IsAny<String>(), It.IsAny<Dictionary<String, Object>>()))
                .Returns(_email);

            _window = new ReleaseWindow
            {
                Sprint = RandomData.RandomString(5, 8),
                StartTime = SystemTime.Now.AddHours(3),
                Products = new[] { RandomData.RandomString(5) },
                ReleaseType = ReleaseType.Scheduled
            };
            _releaseWindowGatewayMock.Setup(r => r.GetByExternalId(_window.ExternalId, false, It.IsAny<bool>()))
                .Returns(_window);

            _account = new Account
            {
                ExternalId = Guid.NewGuid(),
                FullName = RandomData.RandomString(15, 34),
                Email = RandomData.RandomEmail()
            };

            _signOffs = new List<SignOff>
            {
                new SignOff
                {
                    Signer = new Account {FullName = RandomData.RandomString(20, 25), Email = RandomData.RandomEmail()}
                }
                ,
                new SignOff
                {
                    Signer = new Account {FullName = RandomData.RandomString(20, 25), Email = RandomData.RandomEmail()}
                }
            };
            _signOffGatewayMock.Setup(s => s.GetSignOffs(_window.ExternalId)).Returns(_signOffs);

            _members = new List<Account>
            {
                new Account {FullName = RandomData.RandomString(20, 25), Email = RandomData.RandomEmail()}
            };
            _members.AddRange(_signOffs.Select(x => x.Signer));

            base.TestInitialize();
        }

        [Test, ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void Handle_ShouldThrowException_WhenSignersAddedAndReleaseWindowNotFound()
        {
            Sut.Handle(
                new ReleaseSignersAddedEvent { ReleaseWindowId = Guid.NewGuid() });
        }

        [Test]
        public void Handle_ShouldSendEmail_WhenSignersAdded()
        {
            var evnt = new ReleaseSignersAddedEvent
            {
                ReleaseWindowId = _window.ExternalId,
                SignOffs =
                    new List<SignOff>
                    {
                        new SignOff
                        {
                            Signer =
                                new Account
                                {
                                    FullName = RandomData.RandomString(11, 33),
                                    Email = RandomData.RandomEmail(),
                                    ExternalId = Guid.NewGuid()
                                }
                        },
                        new SignOff
                        {
                            Signer =
                                new Account
                                {
                                    FullName = RandomData.RandomString(11, 33),
                                    Email = RandomData.RandomEmail(),
                                    ExternalId = Guid.NewGuid()
                                }
                        }
                    }
            };

            _accountNotificationGatewayMock.Setup(x => x.GetSubscribers(NotificationType.Signing, _window.Products))
                .Returns(evnt.SignOffs.Select(s => s.Signer).ToList());

            Sut.Handle(evnt);

            _accountNotificationGatewayMock.Verify(x => x.GetSubscribers(NotificationType.Signing, _window.Products));
            _releaseWindowGatewayMock.Verify(r => r.GetByExternalId(_window.ExternalId, false, It.IsAny<bool>()));
            _emailTextProviderMock.Verify(e =>
                e.GetText("SignOffAddedToReleaseWindowEmail",
                    It.Is<Dictionary<String, Object>>(d =>
                        d.Any(s => s.Key == "Assignee" && s.Value.ToString() == evnt.SignOffs[0].Signer.FullName) &&
                        //d.Any(s => s.Key == "Products" && s.Value.ToString() == string.Join(", ", _window.Products)) &&
                        d.Any(s => s.Key == "Sprint" && s.Value.ToString() == _window.Sprint) &&
                        d.Any(
                            s =>
                                s.Key == "StartTime" &&
                                s.Value.ToString() ==
                                String.Format("{0:dd/MM/yyyy HH:mm}", _window.StartTime.ToLocalTime())) &&
                        d.Any(s => s.Key == "ReleasePlanUrl")
                        )));
            _emailTextProviderMock.Verify(e =>
                e.GetText("SignOffAddedToReleaseWindowEmail",
                    It.Is<Dictionary<String, Object>>(d =>
                        d.Any(s => s.Key == "Assignee" && s.Value.ToString() == evnt.SignOffs[1].Signer.FullName) &&
                        //d.Any(s => s.Key == "Products" && s.Value.ToString() == string.Join(", ", _window.Products)) &&
                        d.Any(s => s.Key == "Sprint" && s.Value.ToString() == _window.Sprint) &&
                        d.Any(
                            s =>
                                s.Key == "StartTime" &&
                                s.Value.ToString() ==
                                String.Format("{0:dd/MM/yyyy HH:mm}", _window.StartTime.ToLocalTime())) &&
                        d.Any(s => s.Key == "ReleasePlanUrl")
                        )));
            _emailClientMock.Verify(e => e.Send(evnt.SignOffs[0].Signer.Email, string.Join(", ", _window.Products) + AddedToSignOffsSubject, _email));
            _emailClientMock.Verify(e => e.Send(evnt.SignOffs[1].Signer.Email, string.Join(", ", _window.Products) + AddedToSignOffsSubject, _email));
        }

        [Test, ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void Handle_ShouldThrowException_WhenSignerRemovedAndReleaseWindowNotFound()
        {
            Sut.Handle(
                new RemoveSignOffEvent { ReleaseWindowGuid = Guid.NewGuid() });
        }
        
        [Test]
        public void Handle_ShouldSendEmail_WhenSignerRemoved()
        {
            var evnt = new RemoveSignOffEvent
            {
                ReleaseWindowGuid = _window.ExternalId,
                SignOffId = Guid.NewGuid(),
                AccountId = _account.ExternalId
            };

            var acc = new Account
            {
                FullName = _account.FullName,
                ExternalId = evnt.AccountId,
                Email = _account.Email
            };

            _accountNotificationGatewayMock.Setup(x => x.GetSubscribers(NotificationType.Signing, _window.Products))
                .Returns(new List<Account> {acc});

            Sut.Handle(evnt);
            
            _accountNotificationGatewayMock.Verify(x => x.GetSubscribers(NotificationType.Signing, _window.Products));
            _releaseWindowGatewayMock.Verify(r => r.GetByExternalId(_window.ExternalId, false, false));
            _emailTextProviderMock.Verify(e =>
                e.GetText("SignOffRemovedFromReleaseWindowEmail",
                    It.Is<Dictionary<String, Object>>(d =>
                        d.Any(s => s.Key == "Assignee" && s.Value.ToString() == _account.FullName) &&
                        d.Any(s => s.Key == "Products" && s.Value.ToString() == string.Join(", ", _window.Products)) &&
                        d.Any(s => s.Key == "Sprint" && s.Value.ToString() == _window.Sprint) &&
                        d.Any(
                            s =>
                                s.Key == "StartTime" &&
                                s.Value.ToString() ==
                                String.Format("{0:dd/MM/yyyy HH:mm}", _window.StartTime.ToLocalTime())) &&
                        d.Any(s => s.Key == "ReleasePlanUrl")
                        )));
            _emailClientMock.Verify(e => e.Send(_account.Email, string.Join(", ", _window.Products) + RemovedFromSignOffsSubject, _email));
        }
        
        [Test]
        public void Handle_ShouldSendEmail_WhenReleaseSignedOff()
        {
            var evnt = new ReleaseWindowSignedOffEvent
            {
                ReleaseWindow = _window,
                Context = new EventContext
                {
                    UserId = Guid.NewGuid()
                }
            };

            var acc = new Account
            {
                FullName = _account.FullName,
                Email = _account.Email
            };

            _accountNotificationGatewayMock.Setup(x => x.GetSubscribers(NotificationType.Signing, _window.Products))
                .Returns(new List<Account> { acc });

            Sut.Handle(evnt);

            _accountNotificationGatewayMock.Verify(s => s.GetSubscribers(NotificationType.Signing, _window.Products));
            _emailTextProviderMock.Verify(e =>
                e.GetText("ReleaseWindowFullySignedOffEmail",
                    It.Is<Dictionary<String, Object>>(d =>
                        d.Any(
                            s =>
                                s.Key == "ListOfSignOffs" &&
                                s.Value.ToString() ==
                                string.Format("<table style='border: none'><tr><td style=\"padding-right:200px;\">Signer</td><td>Sign off criteria</td></tr><tr><td>{0}</td></tr></table>",
                                    string.Join("</td></tr><tr><td>",
                                        _signOffs.Select(x => x.Signer.FullName + "</td><td>" + x.Comment)))) &&
                        d.Any(s => s.Key == "Products" && s.Value.ToString() == string.Join(", ", _window.Products)) &&
                        d.Any(s => s.Key == "Sprint" && s.Value.ToString() == _window.Sprint) &&
                        d.Any(
                            s =>
                                s.Key == "StartTime" &&
                                s.Value.ToString() ==
                                String.Format("{0:dd/MM/yyyy HH:mm}", _window.StartTime.ToLocalTime())) &&
                        d.Any(s => s.Key == "ReleasePlanUrl")
                        )));
            _emailClientMock.Verify(e => e.Send(It.Is<List<String>>
                (c => c.Contains(acc.Email))
                , string.Join(", ", _window.Products) + ReleaseSignedOffSubject, _email));
        }
    }
}
