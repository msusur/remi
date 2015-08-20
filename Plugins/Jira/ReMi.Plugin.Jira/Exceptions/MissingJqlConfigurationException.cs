using System;
using System.Collections.Generic;

namespace ReMi.Plugin.Jira.Exceptions
{
    public class MissingJqlConfigurationException : ApplicationException
    {
        public MissingJqlConfigurationException(IEnumerable<Guid> packagesIds)
            : base(FormatMessage(packagesIds))
        {
        }

        public MissingJqlConfigurationException(IEnumerable<Guid> packagesIds, Exception innerException)
            : base(FormatMessage(packagesIds), innerException)
        {
        }

        private static string FormatMessage(IEnumerable<Guid> packagesIds)
        {
            return string.Format("Cannot find jira filter configuration for packages: [{0}]",
                string.Join(", ", packagesIds));
        }

    }
}
