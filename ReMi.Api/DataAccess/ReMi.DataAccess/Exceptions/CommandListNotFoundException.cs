using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.DataAccess.Exceptions
{
    public class CommandListNotFoundException : ApplicationException
    {
        public CommandListNotFoundException(IEnumerable<string> commands)
            : base(FormatMessage(commands))
        {
        }

        public CommandListNotFoundException(IEnumerable<string> commands, Exception innerException)
            : base(FormatMessage(commands), innerException)
        {
        }

        public CommandListNotFoundException(string commands)
            : base(FormatMessage(commands))
        {
        }

        private static string FormatMessage(IEnumerable<string> commands)
        {
            return string.Format("Commands not found. Commands list='{0}'", commands.FormatElements());
        }

        private static string FormatMessage(string commands)
        {
            return string.Format("Commands not found. Commands='{0}'", commands);
        }
    }
}
