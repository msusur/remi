using System;

namespace ReMi.DataAccess.Exceptions
{
    public class AccountIsBlockedException : ApplicationException
    {
        public AccountIsBlockedException(Guid externalId)
            : base(FormatMessage(externalId))
        {
        }

        private static string FormatMessage(Guid externalId)
        {
            return string.Format("Can't use blocked account. ExternalId='{0}'", externalId);
        }
    }
}
