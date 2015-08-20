using System;

namespace ReMi.DataAccess.Exceptions
{
    public class AccountEmailNotFoundException : EntityNotFoundException
    {
        public AccountEmailNotFoundException(string email) :
            base("Account", email)
        {
        }

        public AccountEmailNotFoundException(string email, Exception innerException) : 
            base("Account", email, innerException)
        {
        }
    }
}
