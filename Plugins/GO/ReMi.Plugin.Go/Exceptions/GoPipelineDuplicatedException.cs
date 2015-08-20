using System;

namespace ReMi.Plugin.Go.Exceptions
{
    public class GoPipelineDuplicatedException : ApplicationException
    {
        public GoPipelineDuplicatedException(Guid externalId)
            : base(FormatMessage(externalId))
        {

        }

        public GoPipelineDuplicatedException(string name)
            : base(FormatMessage(name))
        {

        }

        public GoPipelineDuplicatedException(Guid externalId, Exception innerException)
            : base(FormatMessage(externalId), innerException)
        {

        }

        public GoPipelineDuplicatedException(string name, Exception innerException)
            : base(FormatMessage(name), innerException)
        {

        }

        private static string FormatMessage(string name)
        {
            return string.Format("Duplicated pipeline Name: {0}", name);
        }
        private static string FormatMessage(Guid externalId)
        {
            return string.Format("Duplicated pipeline ExternalId: {0}", externalId);
        }
    }
}
