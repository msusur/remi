using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Evt;
using ReMi.BusinessLogic.Evt;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Evt;
using ReMi.DataAccess.Exceptions;
using ReMi.TestUtils.UnitTests;

namespace ReMi.BusinessLogic.Tests.Evt
{
    [TestFixture]
    public class EventlBusinessLogicTests : TestClassFor<EventBusinessLogic>
    {
        private Mock<IEventGateway> _eventGatewayMock;
        private Mock<ISerialization> _serializationMock;

        protected override void TestInitialize()
        {
            _eventGatewayMock = new Mock<IEventGateway>();
            _serializationMock = new Mock<ISerialization>();

            base.TestInitialize();
        }

        protected override EventBusinessLogic ConstructSystemUnderTest()
        {
            return new EventBusinessLogic
            {
                EventGatewayFactory = () => _eventGatewayMock.Object,
                Serialization = _serializationMock.Object
            };
        }

        [Test]
        public void GetEventState_ShouldReturnNotRegistered_WhenInvokedWithNonExistingEventId()
        {
            var eventId = new Guid();

            var state = Sut.GetEventState(eventId);

            Assert.AreEqual(EventStateType.NotRegistered, state);
        }

        [Test]
        public void GetEventState_ShouldReturnExpectedState_WhenInvoked()
        {
            var actualState = RandomData.RandomEnum<EventStateType>();

            var eventId = new Guid();

            _eventGatewayMock.Setup(o => o.GetByExternalId(eventId))
                .Returns(new Event { State = actualState });

            var state = Sut.GetEventState(eventId);

            Console.WriteLine("Actual state: {0}, check state: {1}", actualState, state);
            Assert.AreEqual(actualState, state);
        }

        [Test]
        [ExpectedException(typeof(EventNotFoundException))]
        public void SetEventState_ShouldRaiseException_WhenInvokedForNonExistingEvent()
        {
            var state = RandomData.RandomEnum<EventStateType>();
            var eventId = new Guid();

            Sut.SetEventState(eventId, state, null);
        }

        [Test]
        public void SetEventState_ShouldCallGatewayMethod_WhenInvoked()
        {
            var state = RandomData.RandomEnum<EventStateType>();
            var eventId = new Guid();

            _eventGatewayMock.Setup(o => o.GetByExternalId(eventId))
                .Returns(new Event { ExternalId = eventId });

            Sut.SetEventState(eventId, state, "error");

            _eventGatewayMock.Verify(o => o.SetState(eventId, state, "error"));
        }

        [Test]
        [ExpectedException(typeof(EventDuplicationException))]
        public void StartEventState_ShouldRaiseException_WhenInvokedWithExistingEventId()
        {
            var eventId = new Guid();
            var description = RandomData.RandomString(100);
            var handler = RandomData.RandomString(1, 1024);
            var evnt = Builder<TestEvent>.CreateNew().Build();

            _eventGatewayMock.Setup(o => o.GetByExternalId(eventId))
                .Returns(new Event { ExternalId = eventId });

            Sut.StartEvent(eventId, description, evnt, handler);
        }

        [Test, Ignore("we don't store event data entity in db for now")]
        public void StartEventState_ShouldCallSerialization_WhenInvoked()
        {
            var eventId = new Guid();
            var description = RandomData.RandomString(100);
            var handler = RandomData.RandomString(1, 1024);
            var json = RandomData.RandomString(1, 4096);
            var evnt = Builder<TestEvent>.CreateNew().Build();

            _serializationMock.Setup(o => o.ToJson(It.IsAny<TestEvent>(), null, It.IsAny<bool>())).Returns(json);

            Sut.StartEvent(eventId, description, evnt, handler);

            _serializationMock.Verify(o => o.ToJson(evnt, null, It.IsAny<bool>()));
        }

        [Test]
        public void StartEventState_ShouldCallGatewayMethod_WhenInvoked()
        {
            var eventId = new Guid();
            var description = RandomData.RandomString(100);
            var handler = RandomData.RandomString(1, 1024);
            var json = RandomData.RandomString(1, 4096);
            var evnt = Builder<TestEvent>.CreateNew().Build();

            _serializationMock.Setup(o => o.ToJson(It.IsAny<TestEvent>(), null, It.IsAny<bool>())).Returns(json);

            Sut.StartEvent(eventId, description, evnt, handler);

            _eventGatewayMock.Verify(o => o.Create(eventId, description, null, handler));
        }


        public class TestEvent : IEvent
        {
            public EventContext Context { get; set; }
        }
    }
}
