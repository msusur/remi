using System;
using ReMi.BusinessEntities.Evt;
using ReMi.BusinessLogic.Evt;
using ReMi.Common.WebApi.Tracking;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.EventHandlers
{
    public class EventTrackerHandler : IEventTracker
    {
        private readonly IEventBusinessLogic _businessLogic;

        #region .ctor

        public EventTrackerHandler(IEventBusinessLogic businessLogic)
        {
            _businessLogic = businessLogic;
        }

        #endregion

        public void CreateTracker(Guid eventId, string description)
        {
            CreateTracker(eventId, description, null, null);
        }

        public void CreateTracker(Guid eventId, string description, IEvent evnt, string handler)
        {
            IEventBusinessLogic pollLogic = _businessLogic;

            pollLogic.StartEvent(eventId, description, evnt, handler);
        }

        public void Started(Guid commandId)
        {
            IEventBusinessLogic pollLogic = _businessLogic;

            pollLogic.SetEventState(commandId, EventStateType.Processing, null);
        }

        public void Finished(Guid commandId, string error = null)
        {
            IEventBusinessLogic pollLogic = _businessLogic;

            pollLogic.SetEventState(
                commandId,
                !string.IsNullOrWhiteSpace(error) ? EventStateType.Failed : EventStateType.Success,
                error
            );
        }
    }
}
