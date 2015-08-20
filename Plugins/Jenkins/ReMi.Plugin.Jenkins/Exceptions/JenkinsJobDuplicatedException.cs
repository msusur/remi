using System;

namespace ReMi.Plugin.Jenkins.Exceptions
{
    public class JenkinsJobDuplicatedException : ApplicationException
    {
        public JenkinsJobDuplicatedException(Guid externalId)
            : base(FormatMessage(externalId))
        {

        }

        public JenkinsJobDuplicatedException(string name)
            : base(FormatMessage(name))
        {

        }

        public JenkinsJobDuplicatedException(Guid externalId, Exception innerException)
            : base(FormatMessage(externalId), innerException)
        {

        }

        public JenkinsJobDuplicatedException(string name, Exception innerException)
            : base(FormatMessage(name), innerException)
        {

        }

        private static string FormatMessage(string name)
        {
            return string.Format("Duplicated job Name: {0}", name);
        }
        private static string FormatMessage(Guid externalId)
        {
            return string.Format("Duplicated job ExternalId: {0}", externalId);
        }
    }
}
