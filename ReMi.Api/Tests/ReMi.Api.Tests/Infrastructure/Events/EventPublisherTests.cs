using Moq;
using NUnit.Framework;
using ReMi.Api.Insfrastructure;
using ReMi.Api.Insfrastructure.Security;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;
using ReMi.Common.WebApi.Tracking;
using ReMi.Contracts.Cqrs.Events;
using ReMi.TestUtils.UnitTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;

namespace ReMi.Api.Tests.Infrastructure.Events
{
    public class EventPublisherTests : TestClassFor<EventPublisher>
    {
        private Mock<IDependencyResolver> _dependencyResolverMock;
        private Mock<IEventTracker> _eventTrackerMock;
        private Mock<IHandleEvent<TestEvent>> _eventHandlerMock;
        private Mock<IFrontendNotificator> _frontendNotificatorMock;
        private Mock<ISerialization> _serializationMock;
        private Mock<IApplicationSettings> _applicationSettingsMock;

        protected override EventPublisher ConstructSystemUnderTest()
        {
            return new EventPublisher(_dependencyResolverMock.Object)
            {
                FronendNotificator = _frontendNotificatorMock.Object,
                Serialization = _serializationMock.Object,
                ApplicationSettings = _applicationSettingsMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _eventTrackerMock = new Mock<IEventTracker>();

            _dependencyResolverMock = new Mock<IDependencyResolver>();

            _eventHandlerMock = new Mock<IHandleEvent<TestEvent>>();

            _frontendNotificatorMock = new Mock<IFrontendNotificator>();
            _serializationMock = new Mock<ISerialization>(MockBehavior.Strict);

            _applicationSettingsMock = new Mock<IApplicationSettings>();
            _applicationSettingsMock.SetupGet(x => x.LogJsonFormatted).Returns(true);

            base.TestInitialize();
        }

        [Test]
        public void Publish_ShouldRetrieveHandlers_WhenInvoked()
        {
            _dependencyResolverMock.Setup(o => o.GetService(typeof(IEventTracker))).Returns(_eventTrackerMock.Object);

            Sut.Publish(new TestEvent());

            _dependencyResolverMock.Verify(o => o.GetServices(typeof(IHandleEvent<TestEvent>)));
        }

        [Test]
        public void Publish_ShouldInvokeHandlers_WhenInvoked()
        {
            _dependencyResolverMock.Setup(o => o.GetService(typeof(IEventTracker))).Returns(_eventTrackerMock.Object);
            _dependencyResolverMock.Setup(o => o.GetServices(typeof(IHandleEvent<TestEvent>)))
                .Returns(new[] { _eventHandlerMock.Object });

            var evnt = new TestEvent();
            _serializationMock.Setup(x => x.ToJson(evnt, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");

            var tasks = Sut.Publish(evnt);
            Task.WaitAll(tasks);

            _eventHandlerMock.Verify(o => o.Handle(evnt));
        }

        [Test]
        public void Publish_ShouldInvokeEventTracker_WhenInvoked()
        {
            _dependencyResolverMock.Setup(o => o.GetService(typeof(IEventTracker))).Returns(_eventTrackerMock.Object);
            _dependencyResolverMock.Setup(o => o.GetServices(typeof(IHandleEvent<TestEvent>)))
                .Returns(new[] { _eventHandlerMock.Object });

            var evnt = new TestEvent();
            _serializationMock.Setup(x => x.ToJson(evnt, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");

            var tasks = Sut.Publish(evnt);
            Task.WaitAll(tasks);

            _eventTrackerMock.Verify(
                o =>
                    o.CreateTracker(It.IsAny<Guid>(), evnt.GetType().Name, evnt,
                        _eventHandlerMock.Object.GetType().FullName), Times.Once);
            _eventTrackerMock.Verify(o => o.Started(It.IsAny<Guid>()), Times.Once);
            _eventTrackerMock.Verify(o => o.Finished(It.IsAny<Guid>(), null), Times.Once);
        }

        [Test]
        public void Publish_ShouldReportErrorState_WhenHandlerRaisesError()
        {
            _dependencyResolverMock.Setup(o => o.GetService(typeof(IEventTracker))).Returns(_eventTrackerMock.Object);
            _dependencyResolverMock.Setup(o => o.GetServices(typeof(IHandleEvent<TestEvent>)))
                .Returns(new[] { _eventHandlerMock.Object });

            var evnt = new TestEvent();
            _serializationMock.Setup(x => x.ToJson(evnt, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");

            _eventHandlerMock.Setup(o => o.Handle(evnt)).Throws<Exception>();

            var tasks = Sut.Publish(evnt);
            Task.WaitAll(tasks);

            _eventTrackerMock.Verify(
                o =>
                    o.CreateTracker(It.IsAny<Guid>(), evnt.GetType().Name, evnt,
                        _eventHandlerMock.Object.GetType().FullName), Times.Once);
            _eventTrackerMock.Verify(o => o.Started(It.IsAny<Guid>()), Times.Once);
            _eventTrackerMock.Verify(o => o.Finished(It.IsAny<Guid>(), "Request failed!"), Times.Once);
        }

        [Test]
        public void Publish_ShouldCallFronendNotificator_WhenInvoked()
        {
            _dependencyResolverMock.Setup(o => o.GetService(typeof(IEventTracker))).Returns(_eventTrackerMock.Object);
            _dependencyResolverMock.Setup(o => o.GetServices(typeof(IHandleEvent<TestEvent>)))
                .Returns(new[] { _eventHandlerMock.Object });

            var evnt = new TestEvent();

            var tasks = Sut.Publish(evnt);
            Task.WaitAll(tasks);

            _frontendNotificatorMock.Verify(o => o.NotifyFiltered(evnt), Times.Once);
        }

        [Test]
        public void Publish_ShouldReturnTaskArrayWithOneTask_WhenCommandContextIsEmpty()
        {
            var evnt = new TestEvent(null);

            var tasks = Sut.Publish(evnt);
            Assert.AreEqual(1, tasks.Length);
        }

        [Test]
        public void Publish_ShouldInitializeContextFromPrincipal_WhenCommandContextIsEmpty()
        {
            var account = new Account
            {
                ExternalId = Guid.NewGuid(),
                FullName = RandomData.RandomString(10),
                Email = RandomData.RandomEmail(),
                Role = new Role { Name = RandomData.RandomString(10) }
            };
            var evnt = new TestEvent(null);
            Thread.CurrentPrincipal = new RequestPrincipal(account);

            var tasks = Sut.Publish(evnt);

            Task.WaitAll(tasks);
            _frontendNotificatorMock.Verify(o => o.NotifyFiltered(evnt), Times.Once);
        }
    }

    public class TestEvent : IEvent
    {
        public TestEvent()
        {
            Context = new EventContext { UserId = Guid.NewGuid(), UserName = RandomData.RandomString(15) };
        }

        public TestEvent(EventContext context)
        {
            Context = context;
        }

        public EventContext Context { get; set; }
    }
}
