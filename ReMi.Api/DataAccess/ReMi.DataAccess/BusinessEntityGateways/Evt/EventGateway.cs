using System;
using System.Linq;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Evt;

namespace ReMi.DataAccess.BusinessEntityGateways.Evt
{
    public class EventGateway : BaseGateway, IEventGateway
    {
        public IRepository<Event> EventRepository { get; set; }

        public IRepository<EventHistory> EventHistoryRepository { get; set; }

        public IMappingEngine Mapper { get; set; }

        public BusinessEntities.Evt.Event GetByExternalId(Guid externalId)
        {
            Event result = EventRepository
                .GetAllSatisfiedBy(r => r.ExternalId == externalId)
                .FirstOrDefault();

            return Mapper.Map<Event, BusinessEntities.Evt.Event>(result);
        }

        public void Create(Guid externalId, string description, string data, string handler)
        {
            var Event = new Event
            {
                ExternalId = externalId,
                Description = description,
                Data = data,
                Handler = handler
            };

            EventRepository.Insert(Event);

            var newEntry = new EventHistory
            {
                EventId = Event.EventId,
                State = EventStateType.Waiting,
            };

            EventHistoryRepository.Insert(newEntry);
        }

        public void SetState(Guid externalId, BusinessEntities.Evt.EventStateType businessState, string details)
        {
            var dataState = Mapper.Map<BusinessEntities.Evt.EventStateType, EventStateType>(businessState);

            Event Event = EventRepository.GetSatisfiedBy(p => p.ExternalId == externalId);
            if(Event == null)
                throw new EventNotFoundException(externalId);

            var newEntry = new EventHistory
            {
                EventId = Event.EventId,
                State = dataState,
                Details = details
            };

            EventHistoryRepository.Insert(newEntry);
        }

        public override void OnDisposing()
        {
            EventRepository.Dispose();
            EventHistoryRepository.Dispose();

            base.OnDisposing();
        }
    }
}
