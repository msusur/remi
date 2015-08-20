using System;

namespace ReMi.BusinessEntities.Exceptions
{
    public class FailedToAuthenticateException : ApplicationException
    {
        public FailedToAuthenticateException(string userName)
            : base(FormatMessage(userName))
        {
        }


        public FailedToAuthenticateException(string userName, Exception innerException)
            : base(FormatMessage(userName), innerException)
        {
        }

        private static string FormatMessage(string userName)
        {
            return string.Format("Failed to authenticate user: {0}", userName);
        }
    }
}
