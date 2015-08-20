using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;

namespace ReMi.DataAccess.Exceptions
{
    public class AccountNotFoundException : EntityNotFoundException
    {
        public AccountNotFoundException(Guid accountId) :
            base("Account", accountId)
        {
        }

        public AccountNotFoundException(Guid accountId, Exception innerException) : 
            base("Account", accountId, innerException)
        {
        }

        public AccountNotFoundException(List<string> accounts) :
            base("Account", accounts)
        {
        }

        public AccountNotFoundException(List<string> accounts, Exception innerException) :
            base("Account", accounts, innerException)
        {
        }
    }
}
