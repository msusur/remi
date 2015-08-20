using System;

namespace ReMi.Plugin.Go.Exceptions
{
    public class GoServerNotFoundException : ApplicationException
    {
        public GoServerNotFoundException(string name)
            : base(FormatMessage(name))
        {
        }

        public GoServerNotFoundException(string name, Exception innerException)
            : base(FormatMessage(name), innerException)
        {
        }

        private static string FormatMessage(string name)
        {
            return string.Format("Cannot find GO server name. Check plugin global configuration: {0}", name);
        }
    }
}
