using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.BusinessEntities.Exceptions
{
    public class EmailTemplateHasUnregisteredPlaceholdersException : ApplicationException
    {
        public EmailTemplateHasUnregisteredPlaceholdersException(IEnumerable<string> placeholders)
            : base(FormatMessage(placeholders))
        {
        }

        public EmailTemplateHasUnregisteredPlaceholdersException(IEnumerable<string> placeholders, Exception innerException)
            : base(FormatMessage(placeholders), innerException)
        {
        }

        private static string FormatMessage(IEnumerable<string> placeholders)
        {
            return string.Format("Email template has unregistered placeholders. Placeholders=[{0}]",
                string.Join(",", placeholders.ToArray()));
        }


    }
}
