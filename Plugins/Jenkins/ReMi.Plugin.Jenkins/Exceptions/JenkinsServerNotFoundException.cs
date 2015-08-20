using System;

namespace ReMi.Plugin.Jenkins.Exceptions
{
    public class JenkinsServerNotFoundException : ApplicationException
    {
        public JenkinsServerNotFoundException(string name)
            : base(FormatMessage(name))
        {
        }

        public JenkinsServerNotFoundException(string name, Exception innerException)
            : base(FormatMessage(name), innerException)
        {
        }

        private static string FormatMessage(string name)
        {
            return string.Format("Cannot find Jenkins server name. Check plugin global configuration: {0}", name);
        }
    }
}
