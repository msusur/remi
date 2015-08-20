using System;

namespace ReMi.DataAccess.Exceptions
{
    public class AccountAlreadyExistsException : EntityAlreadyExistsException
    {
        public AccountAlreadyExistsException(Guid accountId)
            : base("Account", accountId)
        {
        }
        public AccountAlreadyExistsException(string accountEmail)
            : base("Account", accountEmail)
        {
        }

        public AccountAlreadyExistsException(Guid accountId, Exception innerException)
            : base("Account", accountId, innerException)
        {
        }

        public AccountAlreadyExistsException(string accountEmail, Exception innerException)
            : base("Account", accountEmail, innerException)
        {
        }
    }
}
