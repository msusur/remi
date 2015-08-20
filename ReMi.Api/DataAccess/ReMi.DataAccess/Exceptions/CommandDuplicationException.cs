using System;

namespace ReMi.DataAccess.Exceptions
{
    public class CommandDuplicationException : EntityAlreadyExistsException
    {
        public CommandDuplicationException(Guid externalId)
            : base("Command", externalId)
        {
        }

        public CommandDuplicationException(Guid externalId, Exception innerException)
            : base("Command", externalId, innerException)
        {
        }
    }
}
