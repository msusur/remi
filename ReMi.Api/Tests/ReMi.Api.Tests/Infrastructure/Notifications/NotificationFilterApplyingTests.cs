using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Api.Insfrastructure.Notifications;
using ReMi.Api.Insfrastructure.Notifications.Filters;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;
using ReMi.TestUtils.UnitTests;
using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace ReMi.Api.Tests.Infrastructure.Notifications
{
    public class NotificationFilterApplyingTests : TestClassFor<NotificationFilterApplying>
    {
        private Mock<IDependencyResolver> _dependencyResolverMock;

        protected override NotificationFilterApplying ConstructSystemUnderTest()
        {
            return new NotificationFilterApplying
            {
                DependencyResolver = _dependencyResolverMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _dependencyResolverMock = new Mock<IDependencyResolver>();

            base.TestInitialize();
        }

        [Test]
        public void Apply_ShouldReturnTrue_WhenNoFilterImplemented()
        {
            var account = Builder<Account>.CreateNew().Build();

            var evnt = new TestEvent();

            var subscriptuions = new List<Subscription> { new Subscription { EventName = evnt.GetType().Name } };

            var result = Sut.Apply(evnt, account, subscriptuions);

            Assert.IsTrue(result);
        }

        [Test]
        public void Apply_ShouldReturnTrue_WhenSubscriptionDoesntHasFilter()
        {
            var account = Builder<Account>.CreateNew().Build();
            var product = RandomData.RandomString(10);
            var evnt = new TestEventWithProductFilter(new[] { product });

            var subscriptuions = new List<Subscription> { new Subscription { EventName = evnt.GetType().Name } };

            var result = Sut.Apply(evnt, account, subscriptuions);

            Assert.IsTrue(result);
        }

        [Test]
        public void Apply_ShouldReturnTrue_WhenFilterImplementationNotFound()
        {
            var account = Builder<Account>.CreateNew().Build();
            var product = RandomData.RandomString(10);
            var evnt = new TestEventWithProductFilter(new[] { product });

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

            Assert.IsTrue(result);
        }

        [Test]
        public void Apply_ShouldReturnFalse_WhenFilterByProductNotEqual()
        {
            var account = Builder<Account>.CreateNew().Build();
            var product = RandomData.RandomString(10);
            var evnt = new TestEventWithProductFilter(new[] { product });

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

            _dependencyResolverMock.Setup(
                o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByProduct>)))
                .Returns(new NotificationFilterByProductApplication());

            var result = Sut.Apply(evnt, account, subscriptuions);

            Assert.IsFalse(result);
        }

        [Test]
        public void Apply_ShouldResolveOnlyFilterApplicationbyProduct_WhenInvoked()
        {
            var account = Builder<Account>.CreateNew().Build();
            var product = RandomData.RandomString(10);
            var evnt = new TestEventWithProductFilter(new[] { product });

            var subscriptuions = new List<Subscription> { new Subscription { EventName = evnt.GetType().Name } };

            Sut.Apply(evnt, account, subscriptuions);

            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByProduct>)), Times.Once);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseWindowIdNotRequestor>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseWindowId>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseType>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseTaskId>)), Times.Never);
        }

        [Test]
        public void Apply_ShouldResolveOnlyFilterApplicationbyReleaseType_WhenInvoked()
        {
            var account = Builder<Account>.CreateNew().Build();
            var releaseType = RandomData.RandomEnum<ReleaseType>();
            var evnt = new TestEventWithReleaseTypeFilter(releaseType);

            var subscriptuions = new List<Subscription> { new Subscription { EventName = evnt.GetType().Name } };

            Sut.Apply(evnt, account, subscriptuions);

            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByProduct>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseWindowIdNotRequestor>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseWindowId>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseType>)), Times.Once);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseTaskId>)), Times.Never);
        }

        [Test]
        public void Apply_ShouldResolveOnlyFilterApplicationByReleaseTypeAndProduct_WhenInvoked()
        {
            var account = Builder<Account>.CreateNew().Build();
            var releaseType = RandomData.RandomEnum<ReleaseType>();
            var product = RandomData.RandomString(10);
            var evnt = new TestEventWithProductReleaesTypeFilter(new[] { product }, releaseType);

            var subscriptuions = new List<Subscription> { new Subscription { EventName = evnt.GetType().Name } };

            Sut.Apply(evnt, account, subscriptuions);

            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByProduct>)), Times.Once);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseWindowId>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseWindowIdNotRequestor>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseType>)), Times.Once);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseTaskId>)), Times.Never);
        }

        [Test]
        public void Apply_ShouldResolveOnlyFilterApplicationByByReleaseWindowIdNotRequestor_WhenInvoked()
        {
            var account = Builder<Account>.CreateNew().Build();
            var releaseWindowId = Guid.NewGuid();
            var evnt = new TestEventWithReleaseWindowIdNotRequestorFilter(releaseWindowId, account);

            var subscriptuions = new List<Subscription> { new Subscription { EventName = evnt.GetType().Name } };

            Sut.Apply(evnt, account, subscriptuions);

            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByProduct>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseWindowId>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseWindowIdNotRequestor>)), Times.Once);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseType>)), Times.Never);
            _dependencyResolverMock.Verify(o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByReleaseTaskId>)), Times.Never);
        }

        [Test]
        public void Apply_ShouldReturnTrue_WhenFilterByProductNotImplemented()
        {
            var account = Builder<Account>.CreateNew().Build();
            var product = RandomData.RandomString(10);
            var evnt = new TestEvent();

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

            _dependencyResolverMock.Setup(
                o => o.GetService(typeof(INotificationFilterApplication<INotificationFilterByProduct>)))
                .Returns(new NotificationFilterByProductApplication());

            var result = Sut.Apply(evnt, account, subscriptuions);

            Assert.IsTrue(result);
        }

        #region Events

        private class TestEvent : IEvent
        {
            public EventContext Context { get; set; }
        }

        private class TestEventWithProductFilter : IEvent, INotificationFilterByProduct
        {
            public EventContext Context { get; set; }

            public IEnumerable<string> Products { get; private set; }

            public TestEventWithProductFilter(IEnumerable<string> products)
            {
                Products = products;
            }
        }

        private class TestEventWithReleaseWindowIdNotRequestorFilter : IEvent, INotificationFilterByReleaseWindowIdNotRequestor
        {
            public EventContext Context { get; set; }

            public Guid ReleaseWindowId { get; set; }

            public Guid RequestorId { get { return Context.UserId; } }

            public TestEventWithReleaseWindowIdNotRequestorFilter(Guid releaseWindowId, Account requestor)
            {
                Context = new EventContext { UserId = requestor.ExternalId };
                ReleaseWindowId = releaseWindowId;
            }
        }


        private class TestEventWithReleaseTypeFilter : IEvent, INotificationFilterByReleaseType
        {
            public EventContext Context { get; set; }

            public TestEventWithReleaseTypeFilter(ReleaseType releaseType)
            {
                ReleaseType = releaseType;
            }

            public ReleaseType ReleaseType { get; private set; }
        }

        private class TestEventWithProductReleaesTypeFilter : IEvent, INotificationFilterByReleaseType, INotificationFilterByProduct
        {
            public EventContext Context { get; set; }

            public TestEventWithProductReleaesTypeFilter(IEnumerable<string> products, ReleaseType releaseType)
            {
                Products = products;

                ReleaseType = releaseType;
            }

            public ReleaseType ReleaseType { get; private set; }
            public IEnumerable<string> Products { get; private set; }
        }

        #endregion
    }
}
