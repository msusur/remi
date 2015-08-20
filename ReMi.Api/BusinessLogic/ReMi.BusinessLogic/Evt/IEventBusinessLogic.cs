using System;
using ReMi.BusinessEntities.Evt;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.BusinessLogic.Evt
{
    public interface IEventBusinessLogic
    {
        EventStateType GetEventState(Guid externalId);

        void StartEvent(Guid externalId, string description, IEvent evt, string handler);
        void SetEventState(Guid externalId, EventStateType state, string details);
    }
}
