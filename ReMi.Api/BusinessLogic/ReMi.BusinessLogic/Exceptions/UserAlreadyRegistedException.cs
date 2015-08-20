using System;

namespace ReMi.BusinessLogic.Exceptions
{
    public class UserAlreadyRegistedException : Exception
    {
        public UserAlreadyRegistedException(Guid accountId)
            : base(FormatMessage(accountId))
        {
        }

        public UserAlreadyRegistedException(Guid accountId, Exception innerException)
            : base(FormatMessage(accountId), innerException)
        {
        }

        private static string FormatMessage(Guid accountId)
        {
            return string.Format("User is already registered: {0}",
                accountId);
        }
    }
}
