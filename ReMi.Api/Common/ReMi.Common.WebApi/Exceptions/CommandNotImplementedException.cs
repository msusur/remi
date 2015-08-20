using System;

namespace ReMi.Common.WebApi.Exceptions
{
    public class CommandNotImplementedException : Exception
    {
        public CommandNotImplementedException(string commandName, Guid commandId)
            : base(FormatMessage(commandName, commandId))
        {
        }
        public CommandNotImplementedException(string commandName, Guid commandId, Exception innerException)
            : base(FormatMessage(commandName, commandId), innerException)
        {
        }

        public static string FormatMessage(string commandName, Guid commandId)
        {
            return string.Format("Command with name '{0}' not implemented. CommandId={1}", commandName, commandId);
        }
    }
}
