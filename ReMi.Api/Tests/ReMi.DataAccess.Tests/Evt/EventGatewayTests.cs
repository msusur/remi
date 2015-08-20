using System;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Evt;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Evt;
using EventStateType = ReMi.BusinessEntities.Evt.EventStateType;

namespace ReMi.DataAccess.Tests.Evt
{
    [TestFixture]
    public class EventGatewayTests : TestClassFor<EventGateway>
    {
        private Mock<IRepository<Event>> _eventRepositoryMock;
        private Mock<IRepository<EventHistory>> _eventHistoryRepositoryMock;

        private Mock<IMappingEngine> _mappingEngineMock;

        protected override EventGateway ConstructSystemUnderTest()
        {
            return new EventGateway
            {
                EventHistoryRepository = _eventHistoryRepositoryMock.Object,
                EventRepository = _eventRepositoryMock.Object,
                Mapper = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _eventRepositoryMock = new Mock<IRepository<Event>>();
            _eventHistoryRepositoryMock = new Mock<IRepository<EventHistory>>();
            _mappingEngineMock = new Mock<IMappingEngine>();

            base.TestInitialize();
        }

        [Test]
        public void GetByExternalId_ShouldReturnNull_WhenEventIdNotExists()
        {
            var eventId = Guid.NewGuid();

            var result = Sut.GetByExternalId(eventId);

            Assert.IsNull(result);
        }

        [Test]
        public void GetByExternalId_ShouldReturnEvent_WhenInvoked()
        {
            var evnt = Builder<Event>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _eventRepositoryMock.SetupEntities(new[] { evnt });

            Sut.GetByExternalId(evnt.ExternalId);

            _mappingEngineMock.Verify(o => o.Map<Event, BusinessEntities.Evt.Event>(
                It.Is<Event>(x =>
                    x.ExternalId == evnt.ExternalId
                    && x.CreatedOn == evnt.CreatedOn
                    && x.Data == evnt.Data
                    && x.Handler == evnt.Handler
                    && x.Description == evnt.Description
                )));
        }

        [Test]
        public void Create_ShouldInsertToEventRepository_WhenInvoked()
        {
            var eventId = Guid.NewGuid();
            var description = RandomData.RandomString(100);
            var data = RandomData.RandomString(1, 4096);
            var handler = RandomData.RandomString(1, 1024);

            Sut.Create(eventId, description, data, handler);

            _eventRepositoryMock.Verify(o => o.Insert(It.Is<Event>(row => row.ExternalId == eventId && row.Description == description)));
        }

        [Test]
        public void Create_ShouldInsertToEventHistoryRepository_WhenInvoked()
        {
            var eventId = Guid.NewGuid();
            var description = RandomData.RandomString(100);
            var data = RandomData.RandomString(1, 4096);
            var handler = RandomData.RandomString(1, 1024);

            _eventRepositoryMock.Setup(o => o.Insert(It.IsAny<Event>()))
                .Callback<Event>(evnt => evnt.EventId = RandomData.RandomInt(1, 100));

            Sut.Create(eventId, description, data, handler);

            _eventHistoryRepositoryMock.Verify(o => o.Insert(It.Is<EventHistory>(row => row.State == DataEntities.Evt.EventStateType.Waiting && row.EventId > 0)));
        }

        [Test]
        public void SetState_ShouldInsertToEventHistoryRepository_WhenInvoked()
        {
            var evnt = Builder<Event>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();
            var businessState = RandomData.RandomEnum<EventStateType>();
            var dataState = RandomData.RandomEnum<DataEntities.Evt.EventStateType>();

            _mappingEngineMock.Setup(o => o.Map<EventStateType, DataEntities.Evt.EventStateType>(It.IsAny<EventStateType>()))
                .Returns(dataState);

            _eventRepositoryMock.SetupEntities(new[] { evnt });

            Sut.SetState(evnt.ExternalId, businessState, null);

            _eventHistoryRepositoryMock.Verify(o => o.Insert(It.Is<EventHistory>(row => row.State == dataState && row.EventId == evnt.EventId)));
        }

        [Test]
        [ExpectedException(typeof(EventNotFoundException))]
        public void SetState_ShouldRaiseException_WhenEventNotExists()
        {
            var businessState = RandomData.RandomEnum<EventStateType>();
            var dataState = RandomData.RandomEnum<DataEntities.Evt.EventStateType>();

            _mappingEngineMock.Setup(o => o.Map<EventStateType, DataEntities.Evt.EventStateType>(It.IsAny<EventStateType>()))
                .Returns(dataState);

            Sut.SetState(Guid.NewGuid(), businessState, null);
        }
    }
}
