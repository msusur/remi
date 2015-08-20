using System;

namespace ReMi.DataAccess.Exceptions
{
    public class EventNotFoundException : EntityNotFoundException
    {
        public EventNotFoundException(Guid externalId)
            : base("Event", externalId)
        {
        }

        public EventNotFoundException(Guid externalId, Exception innerException)
            : base("Event", externalId, innerException)
        {
        }
    }
}
