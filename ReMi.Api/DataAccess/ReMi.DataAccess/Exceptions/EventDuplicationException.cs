using System;

namespace ReMi.DataAccess.Exceptions
{
    public class EventDuplicationException : EntityAlreadyExistsException
    {
        public EventDuplicationException(Guid externalId)
            : base("Event", externalId)
        {
        }

        public EventDuplicationException(Guid externalId, Exception innerException)
            : base("Event", externalId, innerException)
        {
        }
    }
}
