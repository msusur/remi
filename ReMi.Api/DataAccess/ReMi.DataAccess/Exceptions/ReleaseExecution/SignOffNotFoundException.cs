using System;

namespace ReMi.DataAccess.Exceptions.ReleaseExecution
{
    public class SignOffNotFoundException : ApplicationException
    {
        public SignOffNotFoundException(Guid signOffId)
            : base(FormatMessage(signOffId))
        {
        }
        public SignOffNotFoundException(Guid accountId, Guid releaseWindowId)
            : base(FormatMessage(accountId, releaseWindowId))
        {
        }

        public SignOffNotFoundException(Guid signOffId, Exception innerException)
            : base(FormatMessage(signOffId), innerException)
		{
		}

        private static string FormatMessage(Guid signOffId)
        {
            return string.Format("Could not find sign off for id: {0}", signOffId);
        }

        private static string FormatMessage(Guid accountId, Guid releaseWindowId)
        {
            return string.Format("Could not find sign off for account id: {0} and release id: {1}", accountId, releaseWindowId);
        }
    }
}
