using System;

namespace ReMi.DataAccess.Exceptions
{
    public class CommandExecutionNotFoundException : EntityNotFoundException
    {
        public CommandExecutionNotFoundException(Guid externalId)
            : base("Command", externalId)
        {
        }

        public CommandExecutionNotFoundException(Guid externalId, Exception innerException)
            : base("Command", externalId, innerException)
        {
        }
    }
}
