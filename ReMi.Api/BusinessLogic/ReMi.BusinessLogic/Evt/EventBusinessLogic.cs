using System;
using ReMi.BusinessEntities.Evt;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Evt;
using ReMi.DataAccess.Exceptions;

namespace ReMi.BusinessLogic.Evt
{
    public class EventBusinessLogic : IEventBusinessLogic
    {
        public Func<IEventGateway> EventGatewayFactory { get; set; }

        public ISerialization Serialization { get; set; }

        public EventStateType GetEventState(Guid externalId)
        {
            using (IEventGateway eventGateway = EventGatewayFactory())
            {
                //check if Guid is unique
                var evnt = eventGateway.GetByExternalId(externalId);
                if (evnt == null)
                {
                    return EventStateType.NotRegistered;
                }

                return evnt.State;
            }
        }

        public void StartEvent(Guid externalId, string description, IEvent evnt, string handler)
        {
            using (IEventGateway eventGateway = EventGatewayFactory())
            {
                //check if Guid is unique
                var existingEvt = eventGateway.GetByExternalId(externalId);
                if (existingEvt != null)
                {
                    throw new EventDuplicationException(externalId);
                }

                eventGateway.Create(externalId, description, null, handler);
            }
        }

        public void SetEventState(Guid externalId, EventStateType state, string details)
        {
            using (IEventGateway eventGateway = EventGatewayFactory())
            {
                //check if Guid is unique
                var evnt = eventGateway.GetByExternalId(externalId);
                if (evnt == null)
                {
                    throw new EventNotFoundException(externalId);
                }

                eventGateway.SetState(externalId, state, details);
            }
        }
    }
}
