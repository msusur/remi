using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.Api.Insfrastructure.Notifications;
using ReMi.Api.Insfrastructure.Notifications.Filters;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;
using ReMi.TestUtils.UnitTests;
using System.Collections.Generic;

namespace ReMi.Api.Tests.Infrastructure.Notifications
{
    public class NotificationFilterByProductApplicationTests : TestClassFor<NotificationFilterByProductApplication>
    {
        protected override NotificationFilterByProductApplication ConstructSystemUnderTest()
        {
            return new NotificationFilterByProductApplication();
        }

        [Test]
        public void Apply_ShouldReturnFalse_WhenFiltersAreEmpty()
        {
            var account = Builder<Account>.CreateNew().Build();

            var product = RandomData.RandomString(10);
            var evnt = new TestEventWithProductFilter(product);

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
        public void Apply_ShouldReturnTrue_WhenProductEquals()
        {
            var account = Builder<Account>.CreateNew().Build();

            var product = RandomData.RandomString(10);
            var evnt = new TestEventWithProductFilter(product);

            var subscriptuions = new List<Subscription>
            {
                new Subscription
                {
                    EventName = evnt.GetType().Name, 
                    Filters = new SubscriptionFilters
                    {
                        new SubscriptionFilter{ Property = "Products", Value = product}
                    }
                }
            };

            var result = Sut.Apply(evnt, account, subscriptuions);

            Assert.IsTrue(result);
        }

        [Test]
        public void Apply_ShouldReturnFalse_WhenProductNotEquals()
        {
            var account = Builder<Account>.CreateNew().Build();

            var product = RandomData.RandomString(10);
            var evnt = new TestEventWithProductFilter(product);

            var subscriptuions = new List<Subscription>
            {
                new Subscription
                {
                    EventName = evnt.GetType().Name, 
                    Filters = new SubscriptionFilters
                    {
                        new SubscriptionFilter{ Property = "Products", Value = RandomData.RandomString(10)}
                    }
                }
            };

            var result = Sut.Apply(evnt, account, subscriptuions);

            Assert.IsFalse(result);
        }

        [Test]
        public void Apply_ShouldReturnFalse_WhenNoAppropriateFilterFound()
        {
            var account = Builder<Account>.CreateNew().Build();

            var product = RandomData.RandomString(10);
            var evnt = new TestEventWithProductFilter(product);

            var subscriptuions = new List<Subscription>
            {
                new Subscription
                {
                    EventName = evnt.GetType().Name, 
                    Filters = new SubscriptionFilters
                    {
                        new SubscriptionFilter{ Property = RandomData.RandomString(10), Value = RandomData.RandomString(10)}
                    }
                }
            };

            var result = Sut.Apply(evnt, account, subscriptuions);

            Assert.IsFalse(result);
        }

        #region Events

        private class TestEventWithProductFilter : IEvent, INotificationFilterByProduct
        {
            public EventContext Context { get; set; }

            public IEnumerable<string> Products { get; private set; }

            public TestEventWithProductFilter(string product)
            {
                Products = new[] { product };
            }
        }

        #endregion

    }
}
