using System;

namespace ReMi.DataAccess.Exceptions
{
    public class CommandNotFoundException : EntityNotFoundException
    {
        public CommandNotFoundException(int commandId)
            : base("Command", commandId)
        {
        }

        public CommandNotFoundException(int commandId, Exception innerException)
            : base("Command", commandId, innerException)
        {
        }
    }
}
