using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public interface IAccountsGateway : IDisposable
    {
        Session StartSession(Account account, Guid sessionId, int sessionDuration);
        Session SignSession(Account account);
        Session GetSession(Guid externalId);
        IEnumerable<Session> GetSessions(Guid accountId);
        void StopAccountSessions(Guid accountExternalId);
        void ProlongSession(Guid sessionId, int sessionDuration);

        int GetDataAccountId(Guid externalId);
        Account GetAccount(Guid externalId, bool forceCheck = false);
        Account GetAccountByEmail(string email);
        IEnumerable<Account> GetAccountsByProduct(string product);
        IEnumerable<Account> GetAccountsByRole(string roleName);
        IEnumerable<Account> GetAccounts();
        IEnumerable<Account> GetAccounts(IEnumerable<Guid> externalIds);

        Account CreateAccount(Account account, bool createdFromAdminPage = true);
        void UpdateAccount(Account account);
        void UpdateAccountProducts(Account account);
        void UpdateAccountPackages(Guid accountId, IEnumerable<Guid> packageIds, Guid defaultPackageId);

        bool CreateNotExistingAccount(Account account);
        bool CreateNotExistingAccount(Account account, string roleName);
        void CreateNotExistingAccounts(IEnumerable<Account> accounts);
        void CreateNotExistingAccounts(IEnumerable<Account> accounts, string roleName);
        void CreateNotExistingReleaseParticipants(IEnumerable<ReleaseParticipant> releaseParticipants);

        IEnumerable<Account> GetProductOwners(IEnumerable<string> products);
        IEnumerable<Account> GetTeamMembers(String product);
        IEnumerable<Account> GetTeamMembersExcludeReleaseSupport(ReleaseWindow window);

        void AssociateAccountsWithProducts(IEnumerable<Guid> productIds, IEnumerable<string> accountEmails, Func<string, TeamRoleRuleResult> updateRoleFunc);
        void AssociateAccountsWithProduct(IEnumerable<string> accountEmails, Guid releaseWindowId, Func<string, TeamRoleRuleResult> updateRoleFunc);
    }
}
