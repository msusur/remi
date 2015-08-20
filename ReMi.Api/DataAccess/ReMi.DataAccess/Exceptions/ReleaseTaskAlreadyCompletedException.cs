using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMi.DataAccess.Exceptions
{
    public class ReleaseTaskAlreadyCompletedException : ApplicationException
    {
        public ReleaseTaskAlreadyCompletedException(Guid externalId)
            : base(FormatMessage(externalId))
        {
        }

        public ReleaseTaskAlreadyCompletedException(string entityType, Guid externalId, Exception innerException)
             : base(FormatMessage(externalId), innerException)
        {
        }

        private static string FormatMessage(Guid externalId)
        {
            return string.Format("Release Task {0} was already completed", externalId);
        }
    }
}
