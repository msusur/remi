using Moq;
using NUnit.Framework;
using ReMi.Api.Insfrastructure.Notifications;
using ReMi.Api.Insfrastructure.Notifications.Filters;
using ReMi.Api.Insfrastructure.Security;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.Auth;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.Auth;
using ReMi.TestUtils.UnitTests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.Api.Tests.Infrastructure.Notifications
{
    public class SubscriptionManagerTests : TestClassFor<SubscriptionManager>
    {
        private Mock<IAccountsBusinessLogic> _accountsBusinessLogicMock;
        private Mock<ISerialization> _serializationMock;
        private Mock<INotificationFilterApplying> _notificationFilterApplyingMock;
        private Mock<IHandleQuery<GetAccountRequest, GetAccountResponse>> _getAccountQueryMock;

        protected override SubscriptionManager ConstructSystemUnderTest()
        {
            return new SubscriptionManager
            {
                AccountsBusinessLogic = _accountsBusinessLogicMock.Object,
                Serialization = _serializationMock.Object,
                NotificationFilterApplying = _notificationFilterApplyingMock.Object,
                GetAccountQuery = _getAccountQueryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountsBusinessLogicMock = new Mock<IAccountsBusinessLogic>();

            _serializationMock = new Mock<ISerialization>();

            _notificationFilterApplyingMock = new Mock<INotificationFilterApplying>();
            _getAccountQueryMock = new Mock<IHandleQuery<GetAccountRequest, GetAccountResponse>>();

            base.TestInitialize();
        }

        [Test]
        public void Register_ShouldRegisterSubcription_WhenInvoked()
        {
            var connectionId = RandomData.RandomString(63);
            var user = RandomData.RandomString(20);
            var sessionId = Guid.NewGuid();

            var token = HttpTokenHelper.GenerateToken(user, sessionId);

            Sut.Register(connectionId, token);

            Assert.IsTrue(Sut.IsRegistered(token));
            Assert.IsTrue(Sut.IsRegisteredConnectionId(connectionId));
        }

        [Test]
        public void Unregister_ShouldUnregisterSubcriptionByConnectionId_WhenInvoked()
        {
            var connectionId = RandomData.RandomString(63);
            var user = RandomData.RandomString(20);
            var sessionId = Guid.NewGuid();

            var token = HttpTokenHelper.GenerateToken(user, sessionId);

            Sut.Register(connectionId, token);

            Sut.UnRegisterByConnectionId(connectionId);

            Assert.IsFalse(Sut.IsRegistered(token));
            Assert.IsFalse(Sut.IsRegisteredConnectionId(connectionId));
        }

        [Test]
        public void Unregister_ShouldUnregisterSubcriptionByToken_WhenInvoked()
        {
            var connectionId = RandomData.RandomString(63);
            var user = RandomData.RandomString(20);
            var sessionId = Guid.NewGuid();

            var token = HttpTokenHelper.GenerateToken(user, sessionId);

            Sut.Register(connectionId, token);

            Sut.UnRegisterByToken(token);

            Assert.IsFalse(Sut.IsRegistered(token));
            Assert.IsFalse(Sut.IsRegisteredConnectionId(connectionId));
        }

        [Test]
        public void Subscribe_ShouldCallSerialization_WhenInvokedWithFilter()
        {
            var connectionId = RandomData.RandomString(63);
            var user = RandomData.RandomString(20);
            var sessionId = Guid.NewGuid();
            var evnt = new TestEvent();
            var filter = RandomData.RandomString(100);

            _accountsBusinessLogicMock.Setup(o => o.GetSession(sessionId)).Returns(new Session {ExternalId = sessionId});
            _getAccountQueryMock.Setup(o => o.Handle(It.IsAny<GetAccountRequest>()))
                .Returns<Guid>(externalId => new GetAccountResponse {Account = new Account {ExternalId = externalId}});

            var token = HttpTokenHelper.GenerateToken(user, sessionId);

            Sut.Register(connectionId, token);

            Sut.Subscribe(connectionId, evnt.GetType().Name, filter);

            _serializationMock.Verify(o => o.FromJson<FrontendFilter>(filter));
        }

        [Test]
        [ExpectedException]
        public void FilterSubscribers_ShouldRaiseException_WhenSessionIdNotRegistered()
        {
            var connectionId = RandomData.RandomString(63);
            var user = RandomData.RandomString(20);
            var sessionId = Guid.NewGuid();
            var evnt = new TestEvent();

            var token = HttpTokenHelper.GenerateToken(user, sessionId);

            Sut.Register(connectionId, token);
            Sut.Subscribe(connectionId, evnt.GetType().Name, null);

            Sut.FilterSubscribers(evnt);
        }

        [Test]
        [ExpectedException]
        public void FilterSubscribers_ShouldRaiseException_WhenAccountIdNotExists()
        {
            var connectionId = RandomData.RandomString(63);
            var user = RandomData.RandomString(20);
            var sessionId = Guid.NewGuid();
            var evnt = new TestEvent();

            _accountsBusinessLogicMock.Setup(o => o.GetSession(sessionId)).Returns(new Session { ExternalId = sessionId });

            var token = HttpTokenHelper.GenerateToken(user, sessionId);

            Sut.Register(connectionId, token);
            Sut.Subscribe(connectionId, evnt.GetType().Name, null);

            Sut.FilterSubscribers(evnt);
        }

        [Test]
        [Ignore("Needt to figure out how to work with a shared static collection with subscriptions")]
        public void Subscribe_ShouldRegisterSubscriptionOnce_WhenInvokedTwice()
        {
            var connectionId = RandomData.RandomString(63);
            var user = RandomData.RandomString(20);
            var sessionId = Guid.NewGuid();
            var evnt = new TestEvent();

            _accountsBusinessLogicMock.Setup(o => o.GetSession(sessionId)).Returns(new Session { ExternalId = sessionId });
            _getAccountQueryMock.Setup(o => o.Handle(It.IsAny<GetAccountRequest>()))
                .Returns<Guid>(externalId => new GetAccountResponse { Account = new Account { ExternalId = externalId } });

            var token = HttpTokenHelper.GenerateToken(user, sessionId);

            Sut.Register(connectionId, token);

            Sut.Subscribe(connectionId, evnt.GetType().Name, null);
            Sut.Subscribe(connectionId, evnt.GetType().Name, null);

            _notificationFilterApplyingMock.Setup(
                o => o.Apply(evnt, It.IsAny<Account>(), It.IsAny<List<Subscription>>())).Returns(true);

            Assert.AreEqual(1, Sut.FilterSubscribers(evnt).Count());
        }

        [Test]
        [Ignore("Needt to figure out how to work with a shared static collection with subscriptions")]
        public void Subscribe_ShouldRegisterSubscription_WhenInvoked()
        {
            var connectionId = RandomData.RandomString(63);
            var user = RandomData.RandomString(20);
            var sessionId = Guid.NewGuid();
            var evnt = new TestEvent();

            _accountsBusinessLogicMock.Setup(o => o.GetSession(sessionId)).Returns(new Session { ExternalId = sessionId });
            _getAccountQueryMock.Setup(o => o.Handle(It.IsAny<GetAccountRequest>()))
                .Returns<Guid>(externalId => new GetAccountResponse { Account = new Account { ExternalId = externalId } });

            var token = HttpTokenHelper.GenerateToken(user, sessionId);

            Sut.Register(connectionId, token);

            Sut.Subscribe(connectionId, evnt.GetType().Name, null);

            _notificationFilterApplyingMock.Setup(
                o => o.Apply(evnt, It.IsAny<Account>(), It.IsAny<List<Subscription>>())).Returns(true);

            Assert.AreEqual(1, Sut.FilterSubscribers(evnt).Count());
        }


        private class TestEvent : IEvent
        {
            public EventContext Context { get; set; }
        }
    }
}
