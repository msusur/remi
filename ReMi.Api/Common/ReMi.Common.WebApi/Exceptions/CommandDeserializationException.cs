using System;

namespace ReMi.Common.WebApi.Exceptions
{
    public class CommandDeserializationException : ApplicationException
    {
        public CommandDeserializationException(string commandName, Guid commandId)
            : base(FormatMessage(commandName, commandId))
        {
        }
        public CommandDeserializationException(string commandName, Guid commandId, Exception innerException)
            : base(FormatMessage(commandName, commandId), innerException)
        {
        }

        public static string FormatMessage(string commandName, Guid commandId)
        {
            return string.Format("Command '{0}' was not deserialized. CommandId={1}", commandName, commandId);
        }
    }
}
