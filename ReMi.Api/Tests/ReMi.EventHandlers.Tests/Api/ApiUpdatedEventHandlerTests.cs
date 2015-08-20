using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.EventHandlers.Api;
using ReMi.Events.Api;

namespace ReMi.EventHandlers.Tests.Api
{
    public class ApiUpdatedEventHandlerTests : TestClassFor<ApiUpdatedEventHandler>
    {
        private Mock<IAccountNotificationGateway> _accountNotificationGatewayMock;
        private Mock<IEmailService> _emailClientMock;
        private Mock<IEmailTextProvider> _emailTextProviderMock;
        private Account _acc;
        private const string EmailSubject = "ReMi API update";

        protected override ApiUpdatedEventHandler ConstructSystemUnderTest()
        {
            return new ApiUpdatedEventHandler
            {
                AccountNotificationGatewayFactory = () => _accountNotificationGatewayMock.Object,
                EmailService = _emailClientMock.Object,
                EmailTextProvider = _emailTextProviderMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountNotificationGatewayMock = new Mock<IAccountNotificationGateway>();
            _emailClientMock = new Mock<IEmailService>();
            _emailTextProviderMock = new Mock<IEmailTextProvider>();
            _acc = new Account
            {
                Email = RandomData.RandomEmail()
            };
            _accountNotificationGatewayMock.Setup(x => x.GetSubscribers(NotificationType.ApiChange))
                .Returns(new List<Account> {_acc});
            _emailTextProviderMock.Setup(
                e => e.GetText("ApiUpdated", It.IsAny<IEnumerable<KeyValuePair<string, object>>>())).Returns("mail");

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldSendFullEmail_WhenSomeMethodsWereAddedAndRemoved()
        {
            Sut.Handle(new ApiUpdatedEvent
            {
                AddedDescriptions = new List<ApiDescription>
                {
                    new ApiDescription
                    {
                        Url = RandomData.RandomString(17),
                        Method = RandomData.RandomString(4)
                    }
                },
                RemovedDescriptions = new List<ApiDescription>
                {
                    new ApiDescription
                    {
                        Url = RandomData.RandomString(18),
                        Method = RandomData.RandomString(3)
                    }
                }
            });

            _emailClientMock.Verify(
                e =>
                    e.Send(It.Is<IEnumerable<String>>(i => i.Count() == 1 && i.First() == _acc.Email), EmailSubject,
                        "mail"));

            _emailTextProviderMock.Verify(
                e => e.GetText("ApiUpdated", It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "AddedMethods" && pair.Value.ToString().Contains("Added API methods:"))
                    &&
                    x.Any(pair => pair.Key == "RemovedMethods" && pair.Value.ToString().Contains("Removed API methods:"))
                    && x.Any(pair => pair.Key == "RemiApiPageUrl"))));
        }

        [Test]
        public void Handle_ShouldSendEmailWithAddedMethods_WhenSomeMethodsWereAdded()
        {
            Sut.Handle(new ApiUpdatedEvent
            {
                AddedDescriptions = new List<ApiDescription>
                {
                    new ApiDescription
                    {
                        Url = RandomData.RandomString(17),
                        Method = RandomData.RandomString(4)
                    }
                }
            });

            _emailClientMock.Verify(
                e =>
                    e.Send(It.Is<IEnumerable<String>>(i => i.Count() == 1 && i.First() == _acc.Email), EmailSubject,
                        "mail"));

            _emailTextProviderMock.Verify(
                e => e.GetText("ApiUpdated", It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "AddedMethods" && pair.Value.ToString().Contains("Added API methods:"))
                    &&
                    x.Any(pair => pair.Key == "RemovedMethods" && pair.Value.ToString().Equals(String.Empty))
                    && x.Any(pair => pair.Key == "RemiApiPageUrl"))));
        }

        [Test]
        public void Handle_ShouldSendEmailithRemovedMethods_WhenSomeMethodsWereRemoved()
        {
            Sut.Handle(new ApiUpdatedEvent
            {
                RemovedDescriptions = new List<ApiDescription>
                {
                    new ApiDescription
                    {
                        Url = RandomData.RandomString(18),
                        Method = RandomData.RandomString(3)
                    }
                }
            });

            _emailClientMock.Verify(
                 e =>
                     e.Send(It.Is<IEnumerable<String>>(i => i.Count() == 1 && i.First() == _acc.Email), EmailSubject,
                         "mail"));

            _emailTextProviderMock.Verify(
                e => e.GetText("ApiUpdated", It.Is<IEnumerable<KeyValuePair<string, object>>>(x =>
                    x.Any(pair => pair.Key == "AddedMethods" && pair.Value.ToString().Equals(String.Empty))
                    &&
                    x.Any(pair => pair.Key == "RemovedMethods" && pair.Value.ToString().Contains("Removed API methods:"))
                    && x.Any(pair => pair.Key == "RemiApiPageUrl"))));
        }
    }
}
