using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.Api.Insfrastructure.Notifications;
using ReMi.Api.Insfrastructure.Notifications.Filters;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;
using ReMi.TestUtils.UnitTests;
using System;
using System.Collections.Generic;

namespace ReMi.Api.Tests.Infrastructure.Notifications
{
    public class NotificationFilterForUserApplicationTests : TestClassFor<NotificationFilterForUserApplication>
    {
        protected override NotificationFilterForUserApplication ConstructSystemUnderTest()
        {
            return new NotificationFilterForUserApplication();
        }

        [Test]
        public void Apply_ShouldReturnFalse_WhenFiltersAreEmpty()
        {
            var account = Builder<Account>.CreateNew()
                .With(x => x.ExternalId = Guid.NewGuid())
                .Build();

            var evnt = new TestEventWithAccountFilter(account);

            var subscriptuions = new List<Subscription>
            {
                new Subscription
                {
                    EventName = evnt.GetType().Name, 
                    Filters = new SubscriptionFilters()
                }
            };

            var result = Sut.Apply(evnt, account, subscriptuions);

            Assert.IsFalse(result);
        }

        [Test]
        public void Apply_ShouldReturnFalse_WhenAccountNotSame()
        {
            var account = Builder<Account>.CreateNew()
                .With(x => x.ExternalId = Guid.NewGuid())
                .Build();

            var evnt = new TestEventWithAccountFilter(account);

            var subscriptuions = new List<Subscription>
            {
                new Subscription
                {
                    EventName = evnt.GetType().Name, 
                    Filters = new SubscriptionFilters()
                }
            };

            var result = Sut.Apply(evnt, new Account { ExternalId = Guid.NewGuid() }, subscriptuions);

            Assert.IsFalse(result);
        }

        #region Events

        private class TestEventWithAccountFilter : IEvent, INotificationFilterForUser
        {
            public EventContext Context { get; set; }

            public Guid AccountId { get; private set; }

            public TestEventWithAccountFilter(Account account)
            {
                AccountId = account.ExternalId;
            }

        }

        #endregion

    }
}
