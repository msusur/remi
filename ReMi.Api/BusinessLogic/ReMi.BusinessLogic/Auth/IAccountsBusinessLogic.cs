using ReMi.BusinessEntities.Auth;
using System;
using System.Collections.Generic;

namespace ReMi.BusinessLogic.Auth
{
    public interface IAccountsBusinessLogic
    {
        Session SignSession(string accountName, string password);

        List<Account> GetAccounts();
        List<Account> GetAccountsByRole(string roleName);

        Session GetSession(Guid externalId);

        void CreateAccount(Account account);
        void UpdateAccount(Account account);

        List<Account> SearchAccounts(string criteria);

        void AssociateAccountsWithProduct(IEnumerable<string> accountEmails, Guid releaseWindowId);
    }
}
